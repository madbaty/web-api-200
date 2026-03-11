using Marten;
using Software.Api.Vendors.Data;
using SoftwareShared.Messages;

namespace Software.Api.CatalogItems;

public class VendorExistsEndpointFilter(IDocumentSession session) : IEndpointFilter
{
    public const string VendorKey = "VendorEntity";
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var vendorId = context.GetArgument<Guid>(0); // positional
        var token = context.HttpContext.RequestAborted;
        // make an HTTP HEAD request to vendor-api/vendors/{id}

        var vendor = await session.LoadAsync<Vendor>(vendorId);
        if(vendor is null )
        {
            return TypedResults.NotFound("No Vendor with that Id");
        }
        context.HttpContext.Items[VendorKey] = vendor;
        return await next(context);
    }
}
