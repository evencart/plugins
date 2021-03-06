﻿using System;
using System.Collections.Generic;
using EvenCart.Data.Entity.Payments;
using Genesis;
using Genesis.Extensions;
using Genesis.Modules.Settings;
using Payments.PaypalWithRedirect.Models;
using PayPal.Core;
using PayPal.v1.PaymentExperience;
using PayPal.v1.Payments;
using Order = EvenCart.Data.Entity.Purchases.Order;
namespace Payments.PaypalWithRedirect.Helpers
{
    public class PaypalHelper
    {
        private static PayPalEnvironment GetEnvironment(PaypalWithRedirectSettings settings)
        {
            if (settings.EnableSandbox)
                return new SandboxEnvironment(settings.ClientId, settings.ClientSecret);

            return new LiveEnvironment(settings.ClientId, settings.ClientSecret);
        }

        public static TransactionResult ProcessApproval(TransactionRequest request, PaypalWithRedirectSettings settings)
        {
            var order = request.Order;
            var payer = new Payer()
            {
                PaymentMethod = "paypal",
            };

            var payment = new Payment()
            {
                Intent = "sale",
                Transactions = new List<Transaction>()
                {
                    new Transaction()
                    {
                        Amount = new Amount()
                        {
                            Total = (request.Amount ?? order.OrderTotal).ToString("N"),
                            Currency = order.CurrencyCode
                        }
                    }
                },
                RedirectUrls = new RedirectUrls()
                {
                    ReturnUrl = GenesisEngine.Instance.RouteUrl(PaypalConfig.PaypalWithRedirectReturnUrlRouteName, new {orderGuid = order.Guid}, absoluteUrl: true),
                    CancelUrl = GenesisEngine.Instance.RouteUrl(PaypalConfig.PaypalWithRedirectCancelUrlRouteName, new { orderGuid = order.Guid }, absoluteUrl: true),
                },
                Payer = payer
            };
            var pcRequest = new PaymentCreateRequest();
            pcRequest.RequestBody(payment);

            var environment = GetEnvironment(settings);

            var client = new PayPalHttpClient(environment);
            var transactionResult = new TransactionResult();
            try
            {
                var response = client.Execute(pcRequest).Result;
                var result = response.Result<Payment>();

                string redirectUrl = null;
                foreach (var link in result.Links)
                {
                    if (link.Rel.Equals("approval_url"))
                    {
                        redirectUrl = link.Href;
                    }
                }

                if (redirectUrl == null)
                {
                    transactionResult.Success = false;
                    transactionResult.Exception = new Exception("Failed to get approval url");
                }
                else
                {
                    transactionResult.Success = true;
                    transactionResult.NewStatus = PaymentStatus.Authorized;
                    transactionResult.Redirect(redirectUrl);
                }
            }
            catch (BraintreeHttp.HttpException ex)
            {
                transactionResult.Exception = ex;
            }

            return transactionResult;
        }

        public static TransactionResult ProcessExecution(Order order, PaymentReturnModel returnModel, PaypalWithRedirectSettings settings)
        {
            var transAmount = order.OrderTotal - order.StoreCreditAmount;
            var payment = new PaymentExecution()
            {
                PayerId = returnModel.PayerId,
                Transactions = new List<CartBase>()
                {
                    new CartBase()
                    {
                        Amount = new Amount()
                        {
                            Currency = order.CurrencyCode,
                            Total = transAmount.ToString("N")
                        }
                    }
                }
            };
            var pcRequest = new PaymentExecuteRequest(returnModel.PaymentId);
            pcRequest.RequestBody(payment);

            var environment = GetEnvironment(settings);

            var client = new PayPalHttpClient(environment);
            var transactionResult = new TransactionResult();
            try
            {
                var response = client.Execute(pcRequest).Result;
                var result = response.Result<Payment>();
                transactionResult.Success = true;
                transactionResult.NewStatus = result.State == "approved" ? PaymentStatus.Complete : PaymentStatus.OnHold;
                transactionResult.OrderGuid = order.Guid;
                transactionResult.TransactionAmount = result.Transactions[0].Amount.Total.GetDecimal();
                transactionResult.TransactionGuid = returnModel.PaymentId;
                transactionResult.TransactionCurrencyCode = result.Transactions[0].Amount.Currency;
                transactionResult.ResponseParameters = new Dictionary<string, object>()
                {
                    { "id", result.Id },
                    { "payerId", returnModel.PayerId },
                    { "paymentId", returnModel.PaymentId },
                    { "createTime", result.CreateTime },
                    { "failureReason", result.FailureReason },
                    { "experienceProfileId", result.ExperienceProfileId },
                    { "noteToPayer", result.NoteToPayer },
                    { "intent", result.Intent },
                    { "state", result.State},
                    { "updateTime", result.UpdateTime },
                    { "saleId", result.Transactions[0].RelatedResources[0].Sale.Id }
                };
                
            }
            catch (BraintreeHttp.HttpException ex)
            {
                transactionResult.Success = false;
                transactionResult.Exception = ex;
            }

            return transactionResult;
        }

        public static TransactionResult ProcessRefund(TransactionRequest request, PaypalWithRedirectSettings settings)
        {
            var order = request.Order;

            var refund = new RefundRequest()
            {
                Amount = new Amount()
                {
                    Total = (request.Amount ?? order.OrderTotal).ToString("N"),
                    Currency = order.CurrencyCode
                },
                Reason = "Admin Initiated Refund",
              
            };
            var saleRefundRequest = new SaleRefundRequest(request.Parameters["saleId"].ToString());
            saleRefundRequest.RequestBody(refund);
            var environment = GetEnvironment(settings);

            var client = new PayPalHttpClient(environment);
            var transactionResult = new TransactionResult();
            try
            {
                var response = client.Execute(saleRefundRequest).Result;
                var result = response.Result<DetailedRefund>();
                transactionResult.Success = true;
                transactionResult.NewStatus = result.State == "approved" || result.State == "pending" ? PaymentStatus.Refunded : PaymentStatus.OnHold;
                transactionResult.OrderGuid = order.Guid;
                transactionResult.TransactionAmount = result.Amount.Total.GetDecimal();
                transactionResult.ResponseParameters = new Dictionary<string, object>()
                {
                    { "id", result.Id },
                    { "parentPayment", result.ParentPayment },
                    { "createTime", result.CreateTime },
                    { "state", result.State},
                    { "updateTime", result.UpdateTime },
                };

            }
            catch (BraintreeHttp.HttpException ex)
            {
                transactionResult.Exception = ex;
            }

            return transactionResult;
        }

        public static string FetchWebProfile(PaypalWithRedirectSettings paypalSettings)
        {
            if (!paypalSettings.CheckoutProfileId.IsNullEmptyOrWhiteSpace())
                return paypalSettings.CheckoutProfileId;

            var webProfileRequest = new WebProfileCreateRequest();
            webProfileRequest.RequestBody(new WebProfile()
            {
                Temporary = false,
                InputFields = new InputFields()
                {
                    NoShipping = 1,
                    AddressOverride = 1
                },
                Name = Guid.NewGuid().ToString()
            });
            var environment = GetEnvironment(paypalSettings);
            var client = new PayPalHttpClient(environment);
            try
            {
                var response = client.Execute(webProfileRequest).Result;
                var result = response.Result<WebProfile>();
                var id = result.Id;
                paypalSettings.CheckoutProfileId = id;
                var settingService = D.Resolve<ISettingService>();
                settingService.Save(paypalSettings, GenesisEngine.Instance.CurrentStore.Id);
                return id;
            }
            catch (BraintreeHttp.HttpException ex)
            {
                return null;
            }
        }
    }
}