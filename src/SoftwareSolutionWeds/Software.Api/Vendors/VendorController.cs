
using Marten;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Software.Api.Clients;
using Software.Api.Vendors.Data;
using Software.Api.Vendors.Models;
using Wolverine;


namespace Software.Api.Vendors;

[ApiController]
public class VendorController(IDocumentSession session) : ControllerBase
{

    //private IDocumentSession session;
    //public VendorController(IDocumentSession session)
    //{
    //    this.session = session;
    //}

    [HttpPost("/vendors")]
    public async Task<ActionResult> AddVendorAsync(
        [FromBody] CreateVendorRequestModel request,
         [FromServices] IMessageBus bus,
        [FromServices] TimeProvider clock,
        [FromServices] IOptions<BlockedVendorsOptions> blockedVendors

        )
    {



        if (blockedVendors.Value.BlockedNames.Any(n => n.Equals(request.Name.Trim(), StringComparison.CurrentCultureIgnoreCase)))
        {
            return BadRequest("can't use that");
        }

        var entityToSave = VendorEntity.From(request, clock);

        session.Store(entityToSave);
  
        var command = new SoftwareShared.Notifications.NotificationRequest { NotificationMessage = "New vendor added " + request.Name };
        
        await bus.PublishAsync(command);

        await session.SaveChangesAsync();

        return Created($"/vendors/{entityToSave.Id}", entityToSave.ToDetails());
    }

    [HttpGet("/vendors")]

    public async Task<ActionResult> GetAllVendorsAsync(CancellationToken token)
    {
        var allVendors = await session.Query<VendorEntity>()
            .ToListAsync(token);
        return Ok(allVendors.Select(v => v.ToSummary()));
    }

    [HttpPut("/vendors/{id:guid}/point-of-contact")]
    [ServiceFilter<VendorExistsFilter>] 
    public async Task<ActionResult> UpdatePoc(Guid id, [FromBody] VendorPointOfContactModel request)
    {


        var vendor = (VendorEntity)HttpContext.Items[VendorExistsFilter.VendorKey]!;     
        vendor.PointOfContact = request;
        session.Store(vendor);
        await session.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("/vendors/{id:guid}")]
    [ServiceFilter<VendorExistsFilter>]
    public async Task<ActionResult> GetVendorByIdAsync(Guid id)
    {

        var vendorEntity = (VendorEntity)HttpContext.Items[VendorExistsFilter.VendorKey]!;
        return Ok(vendorEntity.ToDetails());
    }


}
