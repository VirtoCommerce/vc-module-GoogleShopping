using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Settings;

namespace GoogleShopping.MerchantModule.Web.Services
{
    public class ShoppingManagerSettings : IShoppingSettings
    {
        private readonly string _merchantIdPropertyName;

        private readonly ISettingsManager _settingsManager; 

        public ShoppingManagerSettings(ISettingsManager settingsManager, string merchantIdPropertyName, string code, string description, string logoUrl)
        {
            _merchantIdPropertyName = merchantIdPropertyName;
            _settingsManager = settingsManager;

            Code = code;
            Description = description;
            LogoUrl = logoUrl;
        }

        public ulong MerchantId
        {
            get
            {
                var retVal = _settingsManager.GetValue<ulong>(_merchantIdPropertyName, 0);
                return retVal;
            }
        }

        public string Code { get; }

        public string Description { get; }

        public string LogoUrl { get; }
    }
}