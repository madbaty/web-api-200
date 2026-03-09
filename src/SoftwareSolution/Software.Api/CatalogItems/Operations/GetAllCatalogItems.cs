using Marten;
using Microsoft.AspNetCore.Http;

namespace Software.Api.CatalogItems.Operations;

public static class GetAllCatalogItems
{
    // GET /catalog
    public static async Task<IResult> HandleAsync(IDocumentSession session, CancellationToken token)
    {
        var items = await session.Query<CatalogCreateModel>()
            .Where(c => !c.IsDeprecated)
            .ToListAsync(token);

        return TypedResults.Ok(items);
    }
}
