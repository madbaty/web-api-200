using Alba;
using Software.Api.CatalogItems.Operations;
using Software.Api.Vendors.Models;
using Software.Tests.Fixtures;
using System.Security.Claims;

namespace Software.Tests.CatalogItems;

[Collection("SoftwareSystemTestCollection")]
public class DeprecatingACatalogItem(SoftwareSystemTestFixture fixture) : IClassFixture<SoftwareSystemTestFixture>
{
    [Fact]
    public async Task CanDeprecateCatalogItem()
    {
        var vendor = await CreateVendorAsync("Deprecate Vendor");
        var item = await AddCatalogItemAsync(vendor.Id, "Software To Retire");

        await fixture.Host.Scenario(api =>
        {
            api.WithClaim(new Claim(ClaimTypes.Role, "SoftwareCenter"));
            api.Delete.Url($"/vendors/{vendor.Id}/catalog-items/{item.Id}");
            api.StatusCodeShouldBe(204);
        });

        var response = await fixture.Host.Scenario(api =>
        {
            api.Get.Url($"/vendors/{vendor.Id}/catalog-items");
            api.StatusCodeShouldBeOk();
        });
        var items = response.ReadAsJson<CatalogCreateModel[]>()!;
        Assert.DoesNotContain(items, i => i.Id == item.Id);
    }

    [Fact]
    public async Task ReturnsNotFoundForUnknownItem()
    {
        var vendor = await CreateVendorAsync("Deprecate Not Found Vendor");
        await fixture.Host.Scenario(api =>
        {
            api.WithClaim(new Claim(ClaimTypes.Role, "SoftwareCenter"));
            api.Delete.Url($"/vendors/{vendor.Id}/catalog-items/{Guid.NewGuid()}");
            api.StatusCodeShouldBe(404);
        });
    }

    [Fact]
    public async Task ForbiddenWithoutSoftwareCenterRole()
    {
        await fixture.Host.Scenario(api =>
        {
            api.Delete.Url($"/vendors/{Guid.NewGuid()}/catalog-items/{Guid.NewGuid()}");
            api.StatusCodeShouldBe(403);
        });
    }

    private async Task<VendorDetailsModel> CreateVendorAsync(string name)
    {
        var response = await fixture.Host.Scenario(api =>
        {
            api.WithClaim(new Claim(ClaimTypes.Role, "Manager"));
            api.WithClaim(new Claim(ClaimTypes.Role, "SoftwareCenter"));
            api.Post.Json(new CreateVendorRequestModel
            {
                Name = name,
                Url = $"http://{name.ToLower().Replace(" ", "")}.com",
                PointOfContact = new VendorPointOfContactModel { Name = "Test Contact", Email = "test@example.com", Phone = "555-0100" }
            }).ToUrl("/vendors");
            api.StatusCodeShouldBe(201);
        });
        return response.ReadAsJson<VendorDetailsModel>()!;
    }

    private async Task<CatalogCreateModel> AddCatalogItemAsync(Guid vendorId, string name)
    {
        var response = await fixture.Host.Scenario(api =>
        {
            api.WithClaim(new Claim(ClaimTypes.Role, "SoftwareCenter"));
            api.Post.Json(new CatalogCreateModel { Name = name }).ToUrl($"/vendors/{vendorId}/catalog-items");
            api.StatusCodeShouldBe(201);
        });
        return response.ReadAsJson<CatalogCreateModel>()!;
    }
}
