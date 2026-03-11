using Alba;
using Software.Api.Vendors.Models;
using Software.Tests.Fixtures;
using System.Security.Claims;

namespace Software.Tests.Vendors;

[Collection("SoftwareSystemTestCollection")]
public class GettingVendorById(SoftwareSystemTestFixture fixture) : IClassFixture<SoftwareSystemTestFixture>
{
    [Fact]
    public async Task ReturnsVendorWhenFound()
    {
        var createResponse = await fixture.Host.Scenario(api =>
        {
            api.WithClaim(new Claim(ClaimTypes.Role, "Manager"));
            api.WithClaim(new Claim(ClaimTypes.Role, "SoftwareCenter"));
            api.Post.Json(new CreateVendorRequestModel
            {
                Name = "Get By Id Vendor",
                Url = "http://getbyidvendor.com",
                PointOfContact = new VendorPointOfContactModel { Name = "Jane Doe", Email = "jane@example.com", Phone = "555-0100" }
            }).ToUrl("/vendors");
            api.StatusCodeShouldBe(201);
        });
        var created = createResponse.ReadAsJson<VendorDetailsModel>()!;

        var getResponse = await fixture.Host.Scenario(api =>
        {
            api.Get.Url($"/vendors/{created.Id}");
            api.StatusCodeShouldBeOk();
        });

        var vendor = getResponse.ReadAsJson<VendorDetailsModel>();
        Assert.NotNull(vendor);
        Assert.Equal(created.Id, vendor.Id);
        Assert.Equal("Get By Id Vendor", vendor.Name);
        Assert.Equal(fixture.TestClock, vendor.CreatedAt);
    }

    [Fact]
    public async Task ReturnsNotFoundForUnknownId()
    {
        await fixture.Host.Scenario(api =>
        {
            api.Get.Url($"/vendors/{Guid.NewGuid()}");
            api.StatusCodeShouldBe(404);
        });
    }
}
