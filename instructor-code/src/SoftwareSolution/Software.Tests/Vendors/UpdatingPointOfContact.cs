using Alba;
using Software.Api.Vendors.Models;
using Software.Tests.Fixtures;
using System.Security.Claims;

namespace Software.Tests.Vendors;

[Collection("SoftwareSystemTestCollection")]
public class UpdatingPointOfContact(SoftwareSystemTestFixture fixture) : IClassFixture<SoftwareSystemTestFixture>
{
    [Fact]
    public async Task CanUpdatePointOfContact()
    {
        var createResponse = await fixture.Host.Scenario(api =>
        {
            api.WithClaim(new Claim(ClaimTypes.Role, "Manager"));
            api.WithClaim(new Claim(ClaimTypes.Role, "SoftwareCenter"));
            api.Post.Json(new CreateVendorRequestModel
            {
                Name = "POC Update Vendor",
                Url = "http://pocupdate.com",
                PointOfContact = new VendorPointOfContactModel { Name = "Old Contact", Email = "old@example.com", Phone = "555-0001" }
            }).ToUrl("/vendors");
            api.StatusCodeShouldBe(201);
        });
        var vendor = createResponse.ReadAsJson<VendorDetailsModel>()!;

        var newPoc = new VendorPointOfContactModel { Name = "New Contact", Email = "new@example.com", Phone = "555-0002" };
        await fixture.Host.Scenario(api =>
        {
            api.WithClaim(new Claim(ClaimTypes.Role, "Manager"));
            api.WithClaim(new Claim(ClaimTypes.Role, "SoftwareCenter"));
            api.Put.Json(newPoc).ToUrl($"/vendors/{vendor.Id}/point-of-contact");
            api.StatusCodeShouldBe(204);
        });

        var getResponse = await fixture.Host.Scenario(api =>
        {
            api.Get.Url($"/vendors/{vendor.Id}");
            api.StatusCodeShouldBeOk();
        });
        var updated = getResponse.ReadAsJson<VendorDetailsModel>()!;
        Assert.Equal("New Contact", updated.PointOfContact.Name);
        Assert.Equal("new@example.com", updated.PointOfContact.Email);
    }

    [Fact]
    public async Task ReturnsNotFoundForUnknownVendor()
    {
        var poc = new VendorPointOfContactModel { Name = "Nobody", Email = "nobody@example.com", Phone = "000-0000" };
        await fixture.Host.Scenario(api =>
        {
            api.WithClaim(new Claim(ClaimTypes.Role, "Manager"));
            api.WithClaim(new Claim(ClaimTypes.Role, "SoftwareCenter"));
            api.Put.Json(poc).ToUrl($"/vendors/{Guid.NewGuid()}/point-of-contact");
            api.StatusCodeShouldBe(404);
        });
    }

    [Fact]
    public async Task ForbiddenWithoutManagerRole()
    {
        var poc = new VendorPointOfContactModel { Name = "Someone", Email = "someone@example.com", Phone = "000-0000" };
        await fixture.Host.Scenario(api =>
        {
            api.WithClaim(new Claim(ClaimTypes.Role, "SoftwareCenter")); // team member but not manager
            api.Put.Json(poc).ToUrl($"/vendors/{Guid.NewGuid()}/point-of-contact");
            api.StatusCodeShouldBe(403);
        });
    }
}
