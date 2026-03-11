using Software.Api.CatalogItems.Operations;

namespace Software.Api.CatalogItems;

public static class CatalogExtensions
{

   extension(IEndpointRouteBuilder builder)
    {
        public IEndpointRouteBuilder MapCatalogItemRoutes()
        {
            // TODO: Contemplate Groups.
            var vendorGroup = builder.MapGroup("/vendors").RequireAuthorization();

            var vendorLookupGroup = vendorGroup.MapGroup("");


            vendorLookupGroup.MapPost("/{vendorId:guid}/catalog-items", AddCatalogItem.AddCatalogItemAsync)
                .RequireAuthorization("SoftwareCenter"); // if you got this far, you are authenticated (have a bearer token), so take this personally if you get an error here 403.



            vendorLookupGroup.MapGet("/{vendorId:guid}/catalog-items", GetCatalogItemsByVendor.HandleAsync);

            vendorLookupGroup.MapDelete("/{vendorId:guid}/catalog-items/{itemId:guid}", DeprecateCatalogItem.HandleAsync)
                .RequireAuthorization("SoftwareCenter");

            vendorGroup.MapGet("/catalog", GetAllCatalogItems.HandleAsync).RequireAuthorization();

            return builder;
        }
    }
}
