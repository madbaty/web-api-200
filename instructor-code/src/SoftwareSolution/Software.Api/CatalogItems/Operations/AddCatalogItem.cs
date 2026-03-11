using Marten;
using Microsoft.AspNetCore.Http.HttpResults;
using Software.Api.Vendors.Data;

namespace Software.Api.CatalogItems.Operations;

public static class AddCatalogItem
{
    // POST /vendors/{id}/catalog-items
    public static async Task<Results<Created<CatalogCreateModel>, NotFound<string>>> AddCatalogItemAsync(Guid vendorId, IDocumentSession session, CatalogCreateModel request)
    {
        // all teh stuff we did before, but check to make the vendor exists
        var vendorExists = await session.Query<VendorEntity>().AnyAsync(v => v.Id == vendorId);
        if(vendorExists == false)
        {
            return TypedResults.NotFound("No Vendor with that id");
        }

        request.Id = Guid.NewGuid();
        request.VendorId = vendorId;
        session.Store(request);
        await session.SaveChangesAsync();
        return TypedResults.Created($"/vendors/{vendorId}/catalog-items/{request.Id}", request);
    }
}
