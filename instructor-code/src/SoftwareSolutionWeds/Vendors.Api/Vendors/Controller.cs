using Marten;
using Microsoft.AspNetCore.Mvc;
using SoftwareShared.Messages;
using Wolverine;

namespace Vendors.Api.Vendors;

/* NOTE
 * This will not be feature complete - just enough so you "get it"
 */

[ApiController]
public class Controller(IDocumentSession session) : ControllerBase
{
    // POST

    [HttpPost("/vendors")]
    public async Task<ActionResult> AddVendorAsync(
    [FromBody] CreateVendorRequestModel request,
    [FromServices] IMessageBus bus
    )
    {
        // at this particular date and time a new vendor was created.
        // and a point of contact was assigned to that vendor
        // streamId "the aggregate" - everything that happens to a specific vendor will be correlated by a streamid.
        var vendorId = Guid.NewGuid();
        session.Events.StartStream(vendorId,
            new VendorCreated(request.Name, request.Url),
            new PointOfContactAssignedToAVendor(request.PointOfContact.Name, request.PointOfContact.Email, request.PointOfContact.Phone)
            );
        await session.SaveChangesAsync();
        await bus.PublishAsync(new Vendor {  Id = vendorId, Name =  request.Name });
        return Created($"/vendors/{vendorId}", new { });
    }

   
    [HttpPut("/vendors/{id:guid}/point-of-contact")]
    public async Task<ActionResult> UpdatePoc(Guid id, [FromBody] VendorPointOfContactModel request)
    {
        // probably should check to see if that id is a real vendor...

        session.Events.Append(id, new PointOfContactAssignedToAVendor(request.Name, request.Email, request.Phone));
        await session.SaveChangesAsync();
        return Ok();
    }
   
    [HttpGet("/vendors")]
    public async Task<ActionResult> GetAllVendorsAsync(CancellationToken token)
    {
        var response = await session.Query<VendorListReadModel>().ToListAsync();
        return Ok(response);
    }

    [HttpGet("/vendors/{id:guid}")]
    public async Task<ActionResult> GetVendorByIdAsync(Guid id, CancellationToken token)
    {
        // this is is saying RIGHT NOW go through all the events in this system, one by one, and construct this model.
        // this will be 100% accurate with the state of the events. 
        var vendor = await session.Events.AggregateStreamAsync<VendorDetailsReadModel>(id);
        if (vendor == null)
            return NotFound();

        return Ok(vendor);
    }

    [HttpDelete("/vendors/{id:guid}")]
    public async Task<ActionResult> DeleteAsync(Guid id, [FromServices] IMessageBus bus) {
        session.Events.Append(id, new VendorDeleted());
        await session.SaveChangesAsync();
        await bus.PublishAsync(new VendorTombStone() {  Id  = id });
        return NoContent();
    }
}

public record VendorCreated(string Name, string Url);
public record PointOfContactAssignedToAVendor(string Name, string Email, string Phone);

public record VendorDeleted();