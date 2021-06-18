using Genesis.Infrastructure.Mvc.Models;

namespace Payments.Stripe.Models
{
    public class RedirectToStripeModel : GenesisModel
    {
        public string PublishableKey { get; set; }

        public string SessionId { get; set; }

        public string CancelUrl { get; set; }
    }
}