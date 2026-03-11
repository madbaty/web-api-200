using Marten;
using Microsoft.AspNetCore.Http;

namespace Software.Api.CatalogItems.Operations;

public static class DeprecateCatalogItem
{
    // DELETE /vendors/{vendorId}/catalog-items/{itemId}
    public static async Task<IResult> HandleAsync(Guid vendorId, Guid itemId, IDocumentSession session)
    {
        var item = await session.LoadAsync<CatalogCreateModel>(itemId);
        if (item is null || item.VendorId != vendorId)
        {
            return TypedResults.NotFound("No catalog item with that id for the given vendor");
        }

        item.IsDeprecated = true;
        session.Store(item);
        await session.SaveChangesAsync();

        return TypedResults.NoContent();
    }
}
