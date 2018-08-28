using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using System.Collections.Generic;
using Microsoft.Practices.ObjectBuilder2;
using VirtoCommerce.Platform.Core.Web.Security;

#region Google usings
using Google.Apis.ShoppingContent.v2;
using GoogleShopping.MerchantModule.Web.Providers;
using GoogleShopping.MerchantModule.Web.Services;
using Google.Apis.ShoppingContent.v2.Data;
using GoogleShopping.MerchantModule.Web.Converters;
using GoogleShopping.MerchantModule.Web.Helpers.Interfaces;
using VirtoCommerce.Platform.Core.PushNotifications;
#endregion


namespace GoogleShopping.MerchantModule.Web.Controllers.Api
{
    [ApiExplorerSettings(IgnoreApi=true)]
    [RoutePrefix("api/g")]
    [CheckPermission(Permission = PredefinedPermissions.Manage)]
    public class GoogleShoppingController : ApiController
    {
        private readonly IGoogleProductProvider _productProvider;
        private readonly IShoppingSettings _settingsManager;
        private readonly IPushNotificationManager _pushNotificationManager;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ShoppingContentService _contentService;

        public GoogleShoppingController(
            IShoppingSettings settingsManager, 
            IGoogleProductProvider productProvider,
            IPushNotificationManager pushNotificationManager, 
            IDateTimeProvider dateTimeProvider,
            IGoogleContentServiceProvider googleContentServiceProvider)
        {
            _settingsManager = settingsManager;
            _productProvider = productProvider;
            _pushNotificationManager = pushNotificationManager;
            _dateTimeProvider = dateTimeProvider;
            _contentService = googleContentServiceProvider.GetShoppingContentService();
        }
        
        /// <summary>
        /// Creates a new product in Google Merchant Center that will be based on given VirtoCommerce product.
        /// If the product with given parameters already exists, it will be updated.
        /// </summary>
        /// <param name="productId">Id of VirtoCommerce product to upload to Google Merchant Center.</param>
        /// <returns>Result of product creation.</returns>
        [HttpGet]
        [ResponseType(typeof(void))]
        [Route("products/sync/{productId}")]
        public IHttpActionResult SyncProduct(string productId)
        {
            var updatedProducts = _productProvider.GetProductUpdates(new[] { productId });
            foreach (var product in updatedProducts)
            {
                var productUpdateRequest = _contentService.Products.Insert(product, _settingsManager.MerchantId);
                productUpdateRequest.Execute();
            }

            return Ok();
        }

        /// <summary>
        /// Gets statuses of all products in Google Merchant Center associated with given account.
        /// </summary>
        /// <returns>Retrieved product statuses.</returns>
        [HttpGet]
        [ResponseType(typeof(ProductstatusesListResponse))]
        [Route("products/getstatus")]
        public IHttpActionResult GetProductsStatus()
        {
            var listProductStatusesRequest = _contentService.Productstatuses.List(_settingsManager.MerchantId);
            var response = listProductStatusesRequest.Execute();

            return Ok(response);
        }

        /// <summary>
        /// Finds expired Google Merchant Center products associated with given account and updates information 
        /// with corresponding VirtoCommerce products.
        /// </summary>
        /// <returns>Result of products update.</returns>
        [HttpGet]
        [ResponseType(typeof(void))]
        [Route("products/sync/outdated")]
        public IHttpActionResult UpdateOutdatedProducts()
        {
            var response = _contentService.Productstatuses.List(_settingsManager.MerchantId).Execute();

            var outdatedProductIds = GetOutdatedProductIds(response.Resources);
            if (!outdatedProductIds.Any())
            {
                return StatusCode(HttpStatusCode.NoContent);
            }

            var products = _productProvider.GetProductUpdates(outdatedProductIds);
            foreach (var product in products)
            {
                var productUpdateRequest = _contentService.Products.Insert(product, _settingsManager.MerchantId);
                productUpdateRequest.Execute();
            }

            return Ok();

        }

        /// <summary>
        /// Finds expired Google Merchant Center products associated with given account and updates information 
        /// with corresponding VirtoCommerce products. Update is done using single batch request.
        /// </summary>
        /// <returns>Result of products update.</returns>
        [HttpGet]
        [ResponseType(typeof(ProductsCustomBatchResponse))]
        [Route("products/sync/batch/outdated")]
        public IHttpActionResult BatchOutdatedProducts()
        {
            var response = _contentService.Productstatuses.List(_settingsManager.MerchantId).Execute();

            var outdated = GetOutdatedProductIds(response.Resources);
            if (!outdated.Any())
            {
                return StatusCode(HttpStatusCode.NoContent);
            }

            var productsUpdateRequest = _productProvider.GetProductsBatchRequest(outdated);
            productsUpdateRequest.Entries.ForEach(item => item.MerchantId = _settingsManager.MerchantId);
            var res = _contentService.Products.Custombatch(productsUpdateRequest).Execute();
            return Ok(res);
        }

        /// <summary>
        /// Uploads VirtoCommerce products from given catalog to Google Merchant Center. If one or more products
        /// already exist there, these products' information will be updated.
        /// </summary>
        /// <param name="catalogId">Id of catalog to upload products from.</param>
        /// <returns>Result of products update.</returns>
        [HttpGet]
        [ResponseType(typeof(ProductsCustomBatchResponse))]
        [Route("products/sync/batch/{catalogId}")]
        public IHttpActionResult BatchCatalogProducts(string catalogId)
        {
            var productsUpdateRequest = _productProvider.GetCatalogProductsBatchRequest(catalogId);
            if (!productsUpdateRequest.Entries.Any())
            {
                return StatusCode(HttpStatusCode.NoContent);
            }

            productsUpdateRequest.Entries.ForEach(item => item.MerchantId = _settingsManager.MerchantId);
            var res = _contentService.Products.Custombatch(productsUpdateRequest).Execute();
            return Ok(res);
        }

        /// <summary>
        /// Uploads VirtoCommerce products from given catalog with given category to Google Merchant Center.
        /// If one or more products already exist there, these products' information will be updated.
        /// </summary>
        /// <param name="catalogId">Id of catalog to upload products from.</param>
        /// <param name="categoryId">Id of required product category.</param>
        /// <returns>Result of products update.</returns>
        [HttpGet]
        [Route("products/sync/batch/{catalogId}/{categoryId}")]
        public IHttpActionResult BatchCategoryProducts(string catalogId, string categoryId)
        {
            var products = _productProvider.GetCatalogProductsBatchRequest(catalogId, categoryId);
            if (!products.Entries.Any())
            {
                return StatusCode(HttpStatusCode.NoContent);
            }

            products.Entries.ForEach(item => item.MerchantId = _settingsManager.MerchantId);
            var res = _contentService.Products.Custombatch(products).Execute();
            return Ok(new[] { res.Entries.Count });
        }

        private ICollection<string> GetOutdatedProductIds(IEnumerable<ProductStatus> productStatuses)
        {
            var outdatedProductIds = new Collection<string>();

            foreach (var status in productStatuses)
            {
                var converted = status.ToModuleModel();
                if (_dateTimeProvider.CurrentUtcDateTime > converted.ExpirationDate)
                {
                    outdatedProductIds.Add(converted.ProductId);
                }
            }

            return outdatedProductIds;
        }
    }
}
