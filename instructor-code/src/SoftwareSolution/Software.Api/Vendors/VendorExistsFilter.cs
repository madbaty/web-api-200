using Marten;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Software.Api.Vendors.Data;

namespace Software.Api.Vendors;


// I want to annotate a method that takes a vendor id, and if that vendor doesn't exist, return a 404
public class VendorExistsFilter(IDocumentSession session) : IAsyncActionFilter
{
    public const string VendorKey = "VendorEntity";
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
      
        // read up pattern matching.
        if(context.ActionArguments.TryGetValue("id", out var idValue) && idValue is Guid id)
        {
            var token = context.HttpContext.RequestAborted;
            var vendor = await session.LoadAsync<VendorEntity>(id, token);
            if(vendor is null)
            {
                context.Result = new NotFoundResult();
                return; // YOU SHALL NOT PASS!
            }
            context.HttpContext.Items[VendorKey] = vendor;
        }
        await next();

    }
}
