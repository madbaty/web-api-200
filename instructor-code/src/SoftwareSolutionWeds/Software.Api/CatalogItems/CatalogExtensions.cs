using Software.Api.CatalogItems.Operations;

namespace Software.Api.CatalogItems;

public static class CatalogExtensions
{

   extension(IEndpointRouteBuilder builder)
    {
        public IEndpointRouteBuilder MapCatalogItemRoutes()
        {
           
            var vendorGroup = builder.MapGroup("/vendors");
            
            vendorGroup.MapGet("/catalog", GetAllCatalogItems.HandleAsync);


            var vendorLookupGroup = vendorGroup.MapGroup("").AddEndpointFilter<VendorExistsEndpointFilter>();

            vendorLookupGroup.MapPost("/{vendorId:guid}/catalog-items", AddCatalogItem.AddCatalogItemAsync);

            vendorLookupGroup.MapGet("/{vendorId:guid}/catalog-items", GetCatalogItemsByVendor.HandleAsync);

            vendorLookupGroup.MapDelete("/{vendorId:guid}/catalog-items/{itemId:guid}", DeprecateCatalogItem.HandleAsync);

            return builder;
        }
    }
}
