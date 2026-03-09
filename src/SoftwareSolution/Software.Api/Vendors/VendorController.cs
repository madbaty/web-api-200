
using Marten;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Software.Api.Clients;
using Software.Api.Vendors.Data;
using Software.Api.Vendors.Models;

namespace Software.Api.Vendors;

[ApiController]
public class VendorController(IDocumentSession session) : ControllerBase
{

    [HttpPost("/vendors")]
    [Authorize(Policy = "SoftwareCenterManager")]
    public async Task<ActionResult> AddVendorAsync(
        [FromBody] CreateVendorRequestModel request,
        [FromServices] IDoNotifications api,
        [FromServices] TimeProvider clock
        )
    {
        if(request.Name.Trim().ToLower() == "oracle")
        {
            return BadRequest("We are not allowed to do business with them");
        }

        var entityToSave = VendorEntity.From(request, clock);
        session.Store(entityToSave);
        await api.SendNotification(new SoftwareShared.Notifications.NotificationRequest { Message = "New vendor added " + request.Name });
        await session.SaveChangesAsync();

        return Created($"/vendors/{entityToSave.Id}", entityToSave.ToDetails());
    }

    [HttpGet("/vendors")]
    [Authorize]
    public async Task<ActionResult> GetAllVendorsAsync(CancellationToken token)
    {
        var allVendors = await session.Query<VendorEntity>()
            .ToListAsync(token);
        return Ok(allVendors.Select(v => v.ToSummary()));
    }

    [HttpPut("/vendors/{id:guid}/point-of-contact")]
    [Authorize(Policy = "SoftwareCenterManager")]
    public async Task<ActionResult> UpdatePoc(Guid id, [FromBody] VendorPointOfContactModel request)
    {
        var vendor = await session.LoadAsync<VendorEntity>(id);
        if (vendor is null)
        {
            return NotFound();
        }
        vendor.PointOfContact = request;
        session.Store(vendor);
        await session.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("/vendors/{id:guid}")]
    [Authorize]
    public async Task<ActionResult> GetVendorByIdAsync(Guid id, CancellationToken token)
    {
        var vendorEntity = await session.LoadAsync<VendorEntity>(id, token);
        if (vendorEntity is null)
        {
            return NotFound();
        }
        return Ok(vendorEntity.ToDetails());
    }
}
