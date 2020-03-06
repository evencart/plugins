using EvenCart.Infrastructure.Mvc.Models;

namespace Payments.Stripe.Models
{
    public class RedirectToStripeModel : FoundationModel
    {
        public string PublishableKey { get; set; }

        public string SessionId { get; set; }

        public string CancelUrl { get; set; }
    }
}