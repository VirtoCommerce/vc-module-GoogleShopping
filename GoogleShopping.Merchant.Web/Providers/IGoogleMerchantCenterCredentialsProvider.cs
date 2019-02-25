using Google.Apis.Auth.OAuth2;

namespace GoogleShopping.MerchantModule.Web.Providers
{
    public interface IGoogleMerchantCenterCredentialsProvider
    {
        ServiceAccountCredential GetCredential();
    }
}