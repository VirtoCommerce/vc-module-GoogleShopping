using Google.Apis.Services;
using Google.Apis.ShoppingContent.v2;

namespace GoogleShopping.MerchantModule.Web.Providers
{
    public class ServiceGoogleContentServiceProvider : IGoogleContentServiceProvider
    {
        private readonly IGoogleMerchantCenterCredentialsProvider _credentialsProvider;

        private ShoppingContentService _contentService;

        public ServiceGoogleContentServiceProvider(IGoogleMerchantCenterCredentialsProvider credentialsProvider)
        {
            _credentialsProvider = credentialsProvider;
        }

        public ShoppingContentService GetShoppingContentService()
        {
            if (_contentService == null)
            {
                var credential = _credentialsProvider.GetCredential();

                // Create the service.
                _contentService = new ShoppingContentService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "VirtoCommerce shopping integration"
                });
            }

            return _contentService;
        }
    }
}
