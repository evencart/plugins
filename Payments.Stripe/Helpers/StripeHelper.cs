﻿

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EvenCart.Data.Entity.Payments;
using EvenCart.Services.Extensions;
using EvenCart.Services.Orders;
using EvenCart.Services.Payments;
using EvenCart.Services.Products;
using Genesis;
using Genesis.Extensions;
using Genesis.Modules.Data;
using Genesis.Modules.Localization;
using Genesis.Modules.Logging;
using Genesis.Modules.Meta;
using Genesis.Modules.Users;
using Microsoft.AspNetCore.Http;
using Stripe;
using Stripe.Checkout;
using Address = Genesis.Modules.Addresses.Address;
using Order = EvenCart.Data.Entity.Purchases.Order;
using ProductService = Stripe.ProductService;

namespace Payments.Stripe.Helpers
{
    public class StripeHelper
    {
        private const string StripeCustomerIdKey = "StripeCustomerId";
        private static void InitStripe(StripeSettings stripeSettings, bool secret = false)
        {
            var publishableKey = stripeSettings.EnableTestMode
                ? stripeSettings.TestPublishableKey
                : stripeSettings.PublishableKey;

            var secretKey = stripeSettings.EnableTestMode
                ? stripeSettings.TestSecretKey
                : stripeSettings.SecretKey;
            StripeConfiguration.ApiKey = !secret ? publishableKey : secretKey;
        }

        public static string GetPublishableKey(StripeSettings stripeSettings)
        {
            var publishableKey = stripeSettings.EnableTestMode
                ? stripeSettings.TestPublishableKey
                : stripeSettings.PublishableKey;
            return publishableKey;
        }
        public static TransactionResult ProcessPayment(TransactionRequest request, StripeSettings stripeSettings, ILogger logger)
        {
            InitStripe(stripeSettings);
            var parameters = request.Parameters;
            parameters.TryGetValue("cardNumber", out var cardNumber);
            parameters.TryGetValue("cardName", out var cardName);
            parameters.TryGetValue("expireMonth", out var expireMonthStr);
            parameters.TryGetValue("expireYear", out var expireYearStr);
            parameters.TryGetValue("cvv", out var cvv);

            var order = request.Order;
            //do we have a saved stripe customer id?
            var address = order.BillingAddressSerialized.To<Address>();
            var customerId = GetCustomerId(order.User, null, address);

            var tokenCreateOptions = new TokenCreateOptions
            {
                Card = new CreditCardOptions
                {
                    Number = cardNumber.ToString(),
                    ExpYear = long.Parse(expireYearStr.ToString()),
                    ExpMonth = long.Parse(expireMonthStr.ToString()),
                    Cvc = cvv.ToString(),
                    Name = cardName.ToString(),
                },
                Customer = customerId
            };
            //get token for card
            var tokenService = new TokenService();
            var stripeToken = tokenService.Create(tokenCreateOptions);

            GetFinalAmountDetails(request.Amount ?? order.OrderTotal, order.CurrencyCode, address, out var currencyCode, out var finalAmount);
            var options = new ChargeCreateOptions
            {
                Amount = (long)(finalAmount) * 100,
                Currency = currencyCode,
                Description = stripeSettings.Description,
                Source = stripeToken.Id,
                Capture = !stripeSettings.AuthorizeOnly,
                Customer = customerId
            };
            InitStripe(stripeSettings, true);
            var service = new ChargeService();

            var charge = service.Create(options);
            var processPaymentResult = new TransactionResult()
            {
                ResponseParameters = new Dictionary<string, object>()
                {
                    {"authorizationCode", charge.AuthorizationCode },
                    {"balanceTransactionId", charge.BalanceTransactionId },
                    {"chargeId", charge.Id }
                },
                TransactionGuid = request.TransactionGuid,
                TransactionAmount = (decimal)charge.Amount / 100,
                TransactionCurrencyCode = charge.Currency,
                OrderGuid = order.Guid
            };
            if (charge.Status == "failed")
            {
                logger.Log<TransactionResult>(LogLevel.Warning, "The payment for Order#" + order.Id + " by stripe failed." + charge.FailureMessage);
                processPaymentResult.Success = false;
                processPaymentResult.Exception = new Exception("An error occurred while processing payment");
                return processPaymentResult;
            }

            processPaymentResult.Success = true;
            if (charge.Status == "succeeded")
                processPaymentResult.NewStatus =
                    stripeSettings.AuthorizeOnly ? PaymentStatus.Authorized : PaymentStatus.Complete;
            else
            {
                processPaymentResult.NewStatus = PaymentStatus.Pending;
            }
            return processPaymentResult;
        }

        public static TransactionResult ProcessRefund(TransactionRequest refundRequest, StripeSettings stripeSettings, ILogger logger)
        {
            InitStripe(stripeSettings, true);
            var refundService = new RefundService();
            var order = refundRequest.Order;
            var address = order.BillingAddressSerialized.To<Address>();
            GetFinalAmountDetails(refundRequest.Amount ?? refundRequest.Order.OrderTotal, refundRequest.Order.CurrencyCode, address, out _, out var amount);
            var refundOptions = new RefundCreateOptions
            {
                Charge = refundRequest.GetParameterAs<string>("chargeId"),
                Amount = 100 * (long)(amount),
            };
            var refund = refundService.Create(refundOptions);
            var refundResult = new TransactionResult()
            {
                TransactionGuid = Guid.NewGuid().ToString(),
                ResponseParameters = new Dictionary<string, object>()
                {
                    {"sourceTransferReversalId", refund.SourceTransferReversalId },
                    {"balanceTransactionId", refund.BalanceTransactionId },
                    {"chargeId", refund.ChargeId },
                    {"receiptNumber", refund.ReceiptNumber },
                },
                OrderGuid = refundRequest.Order.Guid,
                TransactionCurrencyCode = refund.Currency,
                TransactionAmount = (decimal)refund.Amount / 100
            };
            if (refund.Status == "failed")
            {
                logger.Log<TransactionResult>(LogLevel.Warning, "The refund for Order#" + refundRequest.Order.Id + " by stripe failed." + refund.FailureReason);
                refundResult.Success = false;
                refundResult.Exception = new Exception("An error occurred while processing refund");
                return refundResult;
            }
            if (refund.Status == "succeeded")
            {
                refundResult.NewStatus = refundRequest.IsPartialRefund ? PaymentStatus.RefundedPartially : PaymentStatus.Refunded;
                refundResult.Success = true;
            }

            return refundResult;
        }

        public static TransactionResult ProcessCapture(TransactionRequest captureRequest, StripeSettings stripeSettings, ILogger logger)
        {
            InitStripe(stripeSettings, true);
            var chargeService = new ChargeService();
            var chargeId = captureRequest.GetParameterAs<string>("chargeId");
            var charge = chargeService.Capture(chargeId, new ChargeCaptureOptions()
            {
                Amount = 100 * (long)(captureRequest.Amount ?? captureRequest.Order.OrderTotal)
            });
            var captureResult = new TransactionResult()
            {
                ResponseParameters = new Dictionary<string, object>()
                {
                    {"chargeId", charge.Id },
                    {"balanceTransactionId", charge.BalanceTransactionId },
                    {"disputeId", charge.DisputeId },
                    {"invoiceId", charge.InvoiceId },
                },
                OrderGuid = captureRequest.Order.Guid,
                TransactionAmount = (decimal)charge.Amount / 100
            };
            if (charge.Status == "failed")
            {
                logger.Log<TransactionResult>(LogLevel.Warning, "The capture for Order#" + captureRequest.Order.Id + " by stripe failed.");
                captureResult.Success = false;
                captureResult.Exception = new Exception("An error occurred while processing capture");
                return captureResult;
            }
            if (charge.Captured.GetValueOrDefault())
            {
                captureResult.NewStatus = PaymentStatus.Complete;
                captureResult.Success = true;
            }

            return captureResult;
        }

        public static TransactionResult ProcessVoid(TransactionRequest captureRequest, StripeSettings stripeSettings, ILogger logger)
        {
            return ProcessRefund(captureRequest, stripeSettings, logger);
        }

        public static TransactionResult CreateSubscription(TransactionRequest request, StripeSettings stripeSettings,
            ILogger logger)
        {
            var order = request.Order;
            InitStripe(stripeSettings);
            var parameters = request.Parameters;
            parameters.TryGetValue("cardNumber", out var cardNumber);
            parameters.TryGetValue("cardName", out var cardName);
            parameters.TryGetValue("expireMonth", out var expireMonthStr);
            parameters.TryGetValue("expireYear", out var expireYearStr);
            parameters.TryGetValue("cvv", out var cvv);

            var paymentMethodService = new PaymentMethodService();
            var paymentMethod = paymentMethodService.Create(new PaymentMethodCreateOptions()
            {
                Card = new PaymentMethodCardCreateOptions()
                {
                    Number = cardNumber.ToString(),
                    ExpYear = long.Parse(expireYearStr.ToString()),
                    ExpMonth = long.Parse(expireMonthStr.ToString()),
                    Cvc = cvv.ToString()
                },
                Type = "card"
            });

            var address = D.Resolve<IDataSerializer>()
                .DeserializeAs<Address>(order.BillingAddressSerialized);

            InitStripe(stripeSettings, true);
            //do we have a saved stripe customer id?
            var customerId = GetCustomerId(order.User, paymentMethod, address);
            var subscriptionItems = new List<SubscriptionItemOptions>();
            var productService = new ProductService();
            var planService = new PlanService();

          
            foreach (var orderItem in order.OrderItems)
            {
                var product = productService.Create(new ProductCreateOptions
                {
                    Name = orderItem.Product.Name,
                    Type = "service"
                });
                var lineTotal = orderItem.Price * orderItem.Quantity + orderItem.Tax;
                GetFinalAmountDetails(lineTotal, order.CurrencyCode, address, out var currencyCode, out var finalAmount);
                var planOptions = new PlanCreateOptions()
                {
                    Nickname = product.Name,
                    Product = product.Id,
                    Amount = (long)(finalAmount),
                    Interval = GetInterval(orderItem.Product.SubscriptionCycle),
                    IntervalCount = orderItem.Product.CycleCount == 0 ? 1 : orderItem.Product.CycleCount,
                    Currency = currencyCode,
                    UsageType = "licensed",
                    TrialPeriodDays = orderItem.Product.TrialDays
                };
                var plan = planService.Create(planOptions);
                subscriptionItems.Add(new SubscriptionItemOptions()
                {
                    Plan = plan.Id,
                    Quantity = orderItem.Quantity
                });
            }
            //create a coupon if any
            var coupon = GetCoupon(order);
            var subscriptionOptions = new SubscriptionCreateOptions()
            {
                Customer = customerId,
                Items = subscriptionItems,
                Metadata = new Dictionary<string, string>()
                {
                    { "orderGuid", order.Guid },
                    { "internalId", order.Id.ToString() },
                    { "isSubscription", bool.TrueString }
                },
                Coupon = coupon?.Id,
                
#if DEBUG
                TrialEnd = DateTime.UtcNow.AddMinutes(5)
#endif
            };
            subscriptionOptions.AddExpand("latest_invoice.payment_intent");

            var subscriptionService = new SubscriptionService();
            var subscription = subscriptionService.Create(subscriptionOptions);
            var processPaymentResult = new TransactionResult()
            {
                OrderGuid = order.Guid,
            };
            if (subscription.Status == "active" || subscription.Status == "trialing")
            {
                processPaymentResult.NewStatus = PaymentStatus.Complete;
                processPaymentResult.TransactionCurrencyCode = order.CurrencyCode;
                processPaymentResult.IsSubscription = true;
                processPaymentResult.TransactionAmount = (subscription.Plan.AmountDecimal / 100) ?? order.OrderTotal;
                processPaymentResult.ResponseParameters = new Dictionary<string, object>()
                {
                    {"subscriptionId", subscription.Id},
                    {"invoiceId", subscription.LatestInvoiceId},
                    {"feePercent", subscription.ApplicationFeePercent},
                    {"collectionMethod", subscription.CollectionMethod},
                    {"metaInfo", subscription.Metadata}
                };
                processPaymentResult.Success = true;
            }
            else
            {
                processPaymentResult.Success = false;
                logger.Log<TransactionResult>(LogLevel.Warning, $"The subscription for Order#{order.Id} by stripe failed with status {subscription.Status}." + subscription.StripeResponse.Content);
            }

            return processPaymentResult;
        }

        public static TransactionResult StopSubscription(TransactionRequest request, StripeSettings stripeSettings, ILogger logger)
        {
            var subscriptionId = request.GetParameterAs<string>("subscriptionId");
            InitStripe(stripeSettings, true);
            var subscriptionService = new SubscriptionService();
            var subscription = subscriptionService.Cancel(subscriptionId, new SubscriptionCancelOptions());
            var stopResult = new TransactionResult()
            {
                TransactionGuid = Guid.NewGuid().ToString(),
                ResponseParameters = new Dictionary<string, object>()
                {
                    {"canceledAt", subscription.CanceledAt },
                },
                OrderGuid = request.Order.Guid,
                TransactionCurrencyCode = request.Order.CurrencyCode,
            };
            if (subscription.Status == "canceled")
            {
                stopResult.Success = true;
                stopResult.NewStatus = PaymentStatus.Complete;
                return stopResult;
            }
            else
            {
                logger.Log<TransactionResult>(LogLevel.Warning, "The subscription cancellation request for Order#" + stopResult.Order.Id + " by stripe failed." + subscription.StripeResponse.Content);
                stopResult.Success = false;
                stopResult.Exception = new Exception("An error occurred while processing refund");
                return stopResult;
            }
        }

        public static TransactionResult CreateSessionRedirect(TransactionRequest request, StripeSettings stripeSettings,
           ILogger logger, bool isSubscription)
        {
            var order = request.Order;
            InitStripe(stripeSettings);

            var address = D.Resolve<IDataSerializer>()
                .DeserializeAs<Address>(order.BillingAddressSerialized);

            InitStripe(stripeSettings, true);
            //do we have a saved stripe customer id?
            var customerId = GetCustomerId(order.User, null, address);

            var subscriptionItems = new List<SessionSubscriptionDataItemOptions>();
            var productService = new ProductService();
            var planService = new PlanService();
            
            foreach (var orderItem in order.OrderItems)
            {
                var product = productService.Create(new ProductCreateOptions
                {
                    Name = orderItem.Product.Name,
                    Type = "service"
                });
                var lineTotal = orderItem.Price * orderItem.Quantity + orderItem.Tax;
                GetFinalAmountDetails(lineTotal, order.CurrencyCode, address, out var currencyCode, out var finalAmount);
                var planOptions = new PlanCreateOptions()
                {
                    Nickname = product.Name,
                    Product = product.Id,
                    Amount = (long)finalAmount,
                    Interval = GetInterval(orderItem.Product.SubscriptionCycle),
                    IntervalCount = orderItem.Product.CycleCount == 0 ? 1 : orderItem.Product.CycleCount,
                    Currency = currencyCode,
                    UsageType = "licensed",
                    TrialPeriodDays = orderItem.Product.TrialDays
                };
                var plan = planService.Create(planOptions);
                subscriptionItems.Add(new SessionSubscriptionDataItemOptions()
                {
                    Plan = plan.Id,
                    Quantity = orderItem.Quantity
                });
            }
            var options = new SessionCreateOptions
            {
                Customer = customerId,
                PaymentMethodTypes = new List<string> {
                    "card",
                },
                SubscriptionData = new SessionSubscriptionDataOptions
                {
                    Items = subscriptionItems,
                    Metadata = new Dictionary<string, string>()
                    {
                        { "orderGuid", order.Guid },
                        { "internalId", order.Id.ToString() },
                        { "isSubscription", isSubscription.ToString() }
                    }
                },
                SuccessUrl = GenesisEngine.Instance.RouteUrl(StripeConfig.StripeReturnUrlRouteName, new { orderGuid = order.Guid }, true),
                CancelUrl = GenesisEngine.Instance.RouteUrl(StripeConfig.StripeCancelUrlRouteName, new { orderGuid = order.Guid }, true),
                Mode = isSubscription ? "subscription" : "payment",
            };
            var service = new SessionService();
            var session = service.Create(options);
            var processPaymentResult = new TransactionResult()
            {
                OrderGuid = order.Guid,
            };

            if (session != null && !session.Id.IsNullEmptyOrWhiteSpace())
            {
                processPaymentResult.NewStatus = PaymentStatus.Processing;
                processPaymentResult.TransactionCurrencyCode = order.CurrencyCode;
                processPaymentResult.IsSubscription = true;
                processPaymentResult.TransactionAmount = order.OrderTotal;
                processPaymentResult.ResponseParameters = new Dictionary<string, object>()
                {
                    {"sessionId", session.Id},
                    {"paymentIntentId", session.PaymentIntentId}
                };
                processPaymentResult.Success = true;
                processPaymentResult.Redirect(GenesisEngine.Instance.RouteUrl(StripeConfig.StripeRedirectToUrlRouteName,
                    new { orderGuid = order.Guid, sessionId = session.Id }));
            }
            else
            {
                processPaymentResult.Success = false;
                logger.Log<TransactionResult>(LogLevel.Warning, $"The session for Order#{order.Id} by stripe redirect failed." + session?.StripeResponse.Content);
            }

            return processPaymentResult;
        }

        public static async void ParseWebhookResponse(HttpRequest responseRequest)
        {
            var json = await new StreamReader(responseRequest.Body).ReadToEndAsync();

            try
            {
                var stripeEvent = EventUtility.ParseEvent(json);
                if (stripeEvent.Type == Events.CustomerSubscriptionTrialWillEnd)
                {
                    //later
                }
                if (stripeEvent.Type == Events.InvoiceUpcoming)
                {
                    //later
                }
                if (stripeEvent.Type == Events.SubscriptionScheduleCanceled)
                {

                }
                if (stripeEvent.Type == Events.InvoicePaymentActionRequired)
                {

                }
                if (stripeEvent.Type == Events.InvoicePaymentFailed)
                {
                    //deactivate the subscription
                    dynamic obj = stripeEvent.Data.Object;
                    string invoiceId = obj.Id;
                    //for some reason, meta data is served from line items
                    Dictionary<string, string> metaData = obj.Lines.Data[0].Metadata;
                    decimal total = obj.Lines.Data[0].Plan.Amount;
                    var currency = obj.Lines.Data[0].Currency;
                    //extract order info
                    if (!metaData.TryGetValue("internalId", out var internalIdStr))
                    {
                        return;
                    }

                    metaData.TryGetValue("isSubscription", out var isSubscriptionStr);
                    var isSubscription = !isSubscriptionStr.IsNullEmptyOrWhiteSpace() &&
                                         isSubscriptionStr == bool.TrueString;

                    var orderId = int.Parse(internalIdStr);
                    var orderService = D.Resolve<IOrderService>();
                    var order = orderService.Get(orderId);
                    if (order == null)
                        return; //was order deleted from db directly. don't do anything. todo: btw should we log this

                    var paymentAccountant = D.Resolve<IPaymentAccountant>();
                    paymentAccountant.ProcessTransactionResult(new TransactionResult()
                    {
                        OrderGuid = order.Guid,
                        Order = order,
                        Success = true,
                        IsSubscription = isSubscription,
                        NewStatus = PaymentStatus.Failed,
                        TransactionAmount = total / 100,
                        TransactionGuid = Guid.NewGuid().ToString(),
                        IsOfflineTransaction = false,
                        TransactionCurrencyCode = currency,
                        ResponseParameters = new Dictionary<string, object>()
                        {
                            { "invoiceId", invoiceId }
                        }
                    });
                }
                if (stripeEvent.Type == Events.InvoicePaymentSucceeded)
                {
                    dynamic obj = stripeEvent.Data.Object;
                    string invoiceId = obj.Id;
                    //for some reason, meta data is served from line items
                    Dictionary<string, string> metaData = obj.Lines.Data[0].Metadata;
                    decimal total = obj.Lines.Data[0].Plan.Amount;
                    var currency = obj.Lines.Data[0].Currency;
                    //extract order info
                    if (!metaData.TryGetValue("internalId", out var internalIdStr))
                    {
                        return;
                    }
                    var orderId = int.Parse(internalIdStr);
                    var orderService = D.Resolve<IOrderService>();
                    var order = orderService.Get(orderId);
                    if (order == null)
                        return; //was order deleted from db directly. don't do anything. todo: btw should we log this
                    var paymentTransactionService = D.Resolve<IPaymentTransactionService>();
                    //get saved transactions
                    var savedTransactions = paymentTransactionService.Get(x => x.OrderGuid == order.Guid && x.PaymentStatus == PaymentStatus.Complete).ToList();
                    if (savedTransactions.Any(x => x.TransactionCodes.ContainsKey("invoiceId") && x.TransactionCodes["invoiceId"].ToString() == invoiceId))
                        return; //already have this invoice, so skip it

                    var paymentAccountant = D.Resolve<IPaymentAccountant>();
                    paymentAccountant.ProcessTransactionResult(new TransactionResult()
                    {
                        OrderGuid = order.Guid,
                        Order = order,
                        Success = true,
                        IsSubscription = true,
                        NewStatus = PaymentStatus.Complete,
                        TransactionAmount = total / 100,
                        TransactionGuid = Guid.NewGuid().ToString(),
                        IsOfflineTransaction = false,
                        TransactionCurrencyCode = currency,
                        ResponseParameters = new Dictionary<string, object>()
                        {
                            { "invoiceId", invoiceId }
                        }
                    });

                }
            }
            catch (StripeException e)
            {

            }
        }
        #region Helpers
        private static string GetInterval(TimeCycle cycle)
        {
            switch (cycle)
            {
                case TimeCycle.Daily:
                    return "day";
                case TimeCycle.Weekly:
                    return "week";
                case TimeCycle.Monthly:
                    return "month";
                case TimeCycle.Yearly:
                    return "year";
                default:
                    throw new ArgumentOutOfRangeException(nameof(cycle), cycle, null);
            }
        }

        private static string GetCustomerId(User user, PaymentMethod paymentMethod, Genesis.Modules.Addresses.Address address)
        {
            var customerId = user.GetPropertyValueAs<string>(StripeCustomerIdKey);
            if (!customerId.IsNullEmptyOrWhiteSpace())
            {
                return customerId;
            }
            var service = new CustomerService();
            var options = new CustomerCreateOptions
            {
                Email = user.Email,
                Address = new AddressOptions()
                {
                    City = address.City,
                    Country = address.Country.Name,
                    Line1 = address.Address1,
                    Line2 = address.Address2,
                    PostalCode = address.ZipPostalCode,
                    State = address.StateProvinceName
                },
                Name = user.Name + "-" + user.Email
            };
            if (paymentMethod != null)
            {
                options.PaymentMethod = paymentMethod.Id;
                options.InvoiceSettings = new CustomerInvoiceSettingsOptions
                {
                    DefaultPaymentMethod = paymentMethod.Id,
                };

            }
            var customer = service.Create(options);
            customerId = customer.Id;
            //save for future use
            user.SetPropertyValue(StripeCustomerIdKey, customerId);
            return customerId;
        }

        private static void GetFinalAmountDetails(decimal inAmount, string inCurrencyCode, Genesis.Modules.Addresses.Address address, out string currencyCode, out decimal amount)
        {
            var priceAccountant = D.Resolve<IPriceAccountant>();
            amount = inAmount;
            currencyCode = inCurrencyCode.ToLower();
            //if target country in India, send converted amount
            if (address.Country.Code == "IN")
            {
                var indianCurrency = D.Resolve<ICurrencyService>()
                    .FirstOrDefault(x => x.IsoCode == "INR");
                if (indianCurrency == null)
                    return;
                amount = 100 * priceAccountant.ConvertCurrency(amount, indianCurrency);
                currencyCode = indianCurrency.IsoCode.ToLower();
                return;
            }

            amount = 100 * amount;
        }

        private static Coupon GetCoupon(Order order)
        {
            Coupon coupon = null;
            if (order.DiscountId.HasValue && order.Discount > 0)
            {
                var couponCode = order.DiscountCoupon;
                if (couponCode.IsNullEmptyOrWhiteSpace())
                {
                    couponCode = "COUPON_" + order.DiscountId;
                }
                //create a coupon
                var options = new CouponCreateOptions
                {
                    Duration = "once",
                    Id = couponCode,
                    AmountOff = (long)order.Discount
                };
                var service = new CouponService();
                coupon = service.Create(options);
            }

            return coupon;
        }
        #endregion
    }
}