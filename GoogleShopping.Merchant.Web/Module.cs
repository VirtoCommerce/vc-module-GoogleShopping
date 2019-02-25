using GoogleShopping.MerchantModule.Web.Controllers.Api;
using GoogleShopping.MerchantModule.Web.Helpers.Implementations;
using GoogleShopping.MerchantModule.Web.Helpers.Interfaces;
using GoogleShopping.MerchantModule.Web.Providers;
using GoogleShopping.MerchantModule.Web.Services;
using Microsoft.Practices.Unity;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.Core.Settings;

namespace GoogleShopping.MerchantModule.Web
{
    public class Module : ModuleBase
    {
        private const string _merchantIdPropertyName = "GoogleShopping.Merchant.MerchantId";

        private const string ServiceAccountPropertyName = "GoogleShopping:MerchantAccount:Email";
        private const string CertificatePathPropertyName = "GoogleShopping:MerchantAccount:Certificate.Path";
        private const string CertificatePassphrasePropertyName = "GoogleShopping:MerchantAccount:Certificate.Passphrase";

        private readonly IUnityContainer _container;

        public Module(IUnityContainer container)
        {
            _container = container;
        }

        #region IModule Members

        public override void Initialize()
        {
            var settingsManager = _container.Resolve<ISettingsManager>();

            var googleShoppingCode = settingsManager.GetValue("GoogleShopping.Merchant.Code", string.Empty);
            var googleShoppingDescription = settingsManager.GetValue("GoogleShopping.Merchant.Description", string.Empty);
            var googleShoppingLogoUrl = settingsManager.GetValue("GoogleShopping.Merchant.LogoUrl", string.Empty);

            var googleShoppingManager = new ShoppingManagerSettings(settingsManager, _merchantIdPropertyName, googleShoppingCode, googleShoppingDescription, googleShoppingLogoUrl);
            var credentialsProvider = new ConfigBasedGoogleMerchantCenterCredentialsProvider(ServiceAccountPropertyName, 
                CertificatePathPropertyName, CertificatePassphrasePropertyName);

            _container.RegisterInstance<IGoogleContentServiceProvider>(new ServiceGoogleContentServiceProvider(credentialsProvider));
            _container.RegisterInstance<IDateTimeProvider>(new DefaultDateTimeProvider());
            _container.RegisterInstance<IShoppingSettings>(googleShoppingManager);
            _container.RegisterType<IGoogleProductProvider, VCGoogleProductProvider>(new ContainerControlledLifetimeManager());

            _container.RegisterType<GoogleShoppingController>();
        }

        #endregion
    }
}
