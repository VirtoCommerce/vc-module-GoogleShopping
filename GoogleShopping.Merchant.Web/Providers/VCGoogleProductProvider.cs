using Google.Apis.ShoppingContent.v2.Data;
using GoogleShopping.MerchantModule.Web.Converters;
using Microsoft.Practices.ObjectBuilder2;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Domain.Pricing.Model;
using VirtoCommerce.Domain.Pricing.Services;
using VirtoCommerce.Platform.Core.Assets;
using GooglePrice = Google.Apis.ShoppingContent.v2.Data.Price;

namespace GoogleShopping.MerchantModule.Web.Providers
{
    public class VCGoogleProductProvider: IGoogleProductProvider
    {
        private readonly IItemService _itemService;
        private readonly IPricingService _pricingService;
        private readonly IBlobUrlResolver _assetUrlResolver;
        private readonly ICatalogSearchService _catalogSearchService;

		public VCGoogleProductProvider(IItemService itemService, IPricingService pricingService, IBlobUrlResolver assetUrlResolver, ICatalogSearchService catalogSearchService)
        {
            _itemService = itemService;
            _pricingService = pricingService;
            _assetUrlResolver = assetUrlResolver;
            _catalogSearchService = catalogSearchService;
        }

        public IEnumerable<Product> GetProductUpdates(IEnumerable<string> ids)
        {
            var retVal = new Collection<Product>();

            var items = _itemService.GetByIds(ids.ToArray(), ItemResponseGroup.ItemLarge);
            foreach (var product in items)
            {
                var converted = product.ToGoogleModel(_assetUrlResolver);
                if (!TryGetProductPrice(converted.Id, out var productPrice))
                    continue;

                converted.Price = productPrice;
                retVal.Add(converted);
            }

            return retVal;
        }

        public ProductsCustomBatchRequest GetProductsBatchRequest(IEnumerable<string> ids)
        {
            var retVal = new ProductsCustomBatchRequest
            {
                Entries = new List<ProductsCustomBatchRequestEntry>()
            };

            var products = GetProductUpdates(ids);
            foreach (var product in products.Select((value, index) => new { Value = value, Index = index }))
            {
                var productEntry = product.Value.ToBatchEntryModel();
                productEntry.BatchId = product.Index + 1;
                retVal.Entries.Add(productEntry); 
            }

            return retVal;
        }

        public ProductsCustomBatchRequest GetCatalogProductsBatchRequest(string catalogId, string categoryId = "")
        {
            var retVal = new ProductsCustomBatchRequest
            {
                Entries = new List<ProductsCustomBatchRequestEntry>()
            };

            var searchCriteria = new SearchCriteria
            {
                CatalogId = catalogId,
                CategoryId = categoryId,
                ResponseGroup = SearchResponseGroup.WithProducts | SearchResponseGroup.WithVariations
            };
            var result = _catalogSearchService.Search(searchCriteria);

            foreach (var product in result.Products.Select((value, index) => new { Value = value, Index = index }))
            {
                var converted = product.Value.ToGoogleModel(_assetUrlResolver);
                if (!TryGetProductPrice(converted.Id, out var productPrice))
                    continue;

                converted.Price = productPrice;

                var val = converted.ToBatchEntryModel();
                val.BatchId = product.Index + 1;
                retVal.Entries.Add(val);
            };
            return retVal;
        }

        private bool TryGetProductPrice(string productId, out GooglePrice productPrice)
        {
            var prices = _pricingService.EvaluateProductPrices(new PriceEvaluationContext { ProductIds = new string[] { productId } });
            if (prices == null)
            {
                productPrice = null;
                return false;
            }

            productPrice = prices.First(x => x.Currency == "USD").ToGoogleModel();
            return true;
        }
    }
}