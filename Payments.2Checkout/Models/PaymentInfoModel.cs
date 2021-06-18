using System.Collections.Generic;
using Genesis.Infrastructure.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Payments.TwoCheckout.Models
{
    public class PaymentInfoModel : GenesisModel
    {
        public PaymentInfoModel()
        {
            AvailableMonths = new List<SelectListItem>();
            AvailableYears = new List<SelectListItem>();
        }
        public IList<SelectListItem> AvailableMonths { get; set; }

        public IList<SelectListItem> AvailableYears { get; set; }

        public int Month { get; set; }

        public int Year { get; set; }

        public bool EnableTestMode { get; set; }

        public string SellerId { get; set; }

        public string PublishableKey { get; set; }
    }
}