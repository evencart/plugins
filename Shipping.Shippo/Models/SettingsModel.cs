﻿using Genesis.Infrastructure.Mvc.Models;
using Genesis.Infrastructure.Mvc.Validator;

namespace Shipping.Shippo.Models
{
    public class SettingsModel : GenesisModel, IRequiresValidations<SettingsModel>
    {
        public bool DebugMode { get; set; }

        public string LiveApiKey { get; set; }

        public string TestApiKey { get; set; }

        public bool UseSinglePackageShipment { get; set; }

        public void SetupValidationRules(ModelValidator<SettingsModel> v)
        {
        }
    }
}