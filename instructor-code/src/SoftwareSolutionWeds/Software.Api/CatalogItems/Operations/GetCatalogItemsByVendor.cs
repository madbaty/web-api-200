using Marten;
using Microsoft.AspNetCore.Http;
using Software.Api.Vendors.Data;

namespace Software.Api.CatalogItems.Operations;

public static class GetCatalogItemsByVendor
{
    // GET /vendors/{vendorId}/catalog-items
    public static async Task<IResult> HandleAsync(Guid vendorId, IDocumentSession session, CancellationToken token)
    {
        var vendorExists = await session.Query<VendorEntity>().AnyAsync(v => v.Id == vendorId, token);
        if (!vendorExists)
        {
            return TypedResults.NotFound("No vendor with that id");
        }

        var items = await session.Query<CatalogCreateModel>()
            .Where(c => c.VendorId == vendorId && !c.IsDeprecated)
            .ToListAsync(token);

        return TypedResults.Ok(items);
    }
}
