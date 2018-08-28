using System;
using System.Security.Cryptography.X509Certificates;
using Google.Apis.Auth.OAuth2;
using Google.Apis.ShoppingContent.v2;
using VirtoCommerce.Platform.Core.Common;

namespace GoogleShopping.MerchantModule.Web.Providers
{
    public class ConfigBasedGoogleMerchantCenterCredentialsProvider : IGoogleMerchantCenterCredentialsProvider
    {
        private readonly string _serviceAccountEmailPropertyName;
        private readonly string _certificatePathPropertyName;
        private readonly string _certificatePassphrasePropertyName;

        public ConfigBasedGoogleMerchantCenterCredentialsProvider(string serviceAccountEmailPropertyName, 
            string certificatePathPropertyName, string certificatePassphrasePropertyName)
        {
            _serviceAccountEmailPropertyName = serviceAccountEmailPropertyName;
            _certificatePathPropertyName = certificatePathPropertyName;
            _certificatePassphrasePropertyName = certificatePassphrasePropertyName;
        }

        public ServiceAccountCredential GetCredential()
        {
            var serviceAccountEmail = ConfigurationHelper.GetAppSettingsValue(_serviceAccountEmailPropertyName);
            var certificatePath = ConfigurationHelper.GetAppSettingsValue(_certificatePathPropertyName);
            var certificatePassphrase = ConfigurationHelper.GetAppSettingsValue(_certificatePassphrasePropertyName);

            var certificate = new X509Certificate2(certificatePath, certificatePassphrase, X509KeyStorageFlags.Exportable);

            var credentialInitializer = new ServiceAccountCredential.Initializer(serviceAccountEmail)
                {
                    Scopes = new[] { ShoppingContentService.Scope.Content }
                }.FromCertificate(certificate);

            return new ServiceAccountCredential(credentialInitializer);
        }
    }
}