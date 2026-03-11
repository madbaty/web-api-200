
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
[Authorize] // every method needs authorization
public class VendorController(IDocumentSession session) : ControllerBase
{

    //private IDocumentSession session;
    //public VendorController(IDocumentSession session)
    //{
    //    this.session = session;
    //}

    [HttpPost("/vendors")]
    [Authorize(Policy = "SoftwareCenterManager")]
    public async Task<ActionResult> AddVendorAsync(
        [FromBody] CreateVendorRequestModel request,
         [FromServices] IMessageBus bus,
        [FromServices] TimeProvider clock,
        [FromServices] IOptions<BlockedVendorsOptions> blockedVendors

        )
    {
        // TODO - this is obviously classroom "slime" - has to be a better way.
        // -- show the options pattern here, which might be a little better, but you need to know that.


        if (blockedVendors.Value.BlockedNames.Any(n => n == request.Name.Trim().ToLower()))
        {
            return BadRequest("can't use that");
        }

        var entityToSave = VendorEntity.From(request, clock);
        // make this new vendor part of a transaction
        session.Store(entityToSave);
        // do this other thing that is in no way part of that transaction
        //await api.SendNotification(new SoftwareShared.Notifications.NotificationRequest { NotificationMessage = "New vendor added " + request.Name });
        var command = new SoftwareShared.Notifications.NotificationRequest { NotificationMessage = "New vendor added " + request.Name };
        //await bus.SendAsync(command); // there will be (there isn't now) some code to handle this.
                                      // await bus.InvokeAsync(command);
        await bus.PublishAsync(command);
        //bus.ScheduleAsync(command, TimeSpan.FromDays(30));
        //session.Store(someNotification);
        // assuming we got here, commit the transaction.
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
    [Authorize(Policy = "SoftwareCenterManager")]
    [ServiceFilter<VendorExistsFilter>]  // hey before you run this, make sure this says it's cool.
    public async Task<ActionResult> UpdatePoc(Guid id, [FromBody] VendorPointOfContactModel request)
    {
        // as much validation that can be done before we get here, means we don't have to repeat that.

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

    // How do we add a catalog item to a vendor
    // do we put the add a thing in here.... or where
}
