using Marten;
using Software.Api.Vendors.Data;

namespace Software.Api.CatalogItems;

public class VendorExistsEndpointFilter(IDocumentSession session) : IEndpointFilter
{
    public const string VendorKey = "VendorEntity";
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var vendorId = context.GetArgument<Guid>(0); // positional
        var token = context.HttpContext.RequestAborted;

        var vendor = await session.LoadAsync<VendorEntity>(vendorId);
        if (vendor is null)
        {
            return TypedResults.NotFound("No Vendor with that Id");
        }
        context.HttpContext.Items[VendorKey] = vendor;
        return await next(context);
    }
}
