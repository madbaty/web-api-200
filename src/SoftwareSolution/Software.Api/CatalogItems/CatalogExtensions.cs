using Software.Api.CatalogItems.Operations;

namespace Software.Api.CatalogItems;

public static class CatalogExtensions
{

   extension(IEndpointRouteBuilder builder)
    {
        public IEndpointRouteBuilder MapCatalogItemRoutes()
        {
            var vendorGroup = builder.MapGroup("/vendors").RequireAuthorization();

            vendorGroup.MapPost("/{vendorId:guid}/catalog-items", AddCatalogItem.AddCatalogItemAsync)
                .RequireAuthorization("SoftwareCenter");

            vendorGroup.MapGet("/{vendorId:guid}/catalog-items", GetCatalogItemsByVendor.HandleAsync);

            vendorGroup.MapDelete("/{vendorId:guid}/catalog-items/{itemId:guid}", DeprecateCatalogItem.HandleAsync)
                .RequireAuthorization("SoftwareCenter");

            builder.MapGet("/catalog", GetAllCatalogItems.HandleAsync).RequireAuthorization();

            return builder;
        }
    }
}
