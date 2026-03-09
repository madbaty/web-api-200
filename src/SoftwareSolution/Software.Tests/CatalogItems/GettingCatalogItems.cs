using Alba;
using Software.Api.CatalogItems.Operations;
using Software.Api.Vendors.Models;
using Software.Tests.Fixtures;
using System.Security.Claims;

namespace Software.Tests.CatalogItems;

[Collection("SoftwareSystemTestCollection")]
public class GettingCatalogItems(SoftwareSystemTestFixture fixture) : IClassFixture<SoftwareSystemTestFixture>
{
    [Fact]
    public async Task CanGetCatalogItemsForAVendor()
    {
        var vendor = await CreateVendorAsync("Get Items Vendor");
        await AddCatalogItemAsync(vendor.Id, "Item One");
        await AddCatalogItemAsync(vendor.Id, "Item Two");

        var response = await fixture.Host.Scenario(api =>
        {
            api.Get.Url($"/vendors/{vendor.Id}/catalog-items");
            api.StatusCodeShouldBeOk();
        });

        var items = response.ReadAsJson<CatalogCreateModel[]>()!;
        Assert.Equal(2, items.Length);
        Assert.All(items, i => Assert.Equal(vendor.Id, i.VendorId));
    }

    [Fact]
    public async Task ReturnsNotFoundForUnknownVendor()
    {
        await fixture.Host.Scenario(api =>
        {
            api.Get.Url($"/vendors/{Guid.NewGuid()}/catalog-items");
            api.StatusCodeShouldBe(404);
        });
    }

    [Fact]
    public async Task CanGetFullCatalog()
    {
        var response = await fixture.Host.Scenario(api =>
        {
            api.Get.Url("/catalog");
            api.StatusCodeShouldBeOk();
        });

        var items = response.ReadAsJson<CatalogCreateModel[]>();
        Assert.NotNull(items);
        Assert.All(items, i => Assert.False(i.IsDeprecated));
    }

    [Fact]
    public async Task DeprecatedItemsAreExcludedFromVendorCatalog()
    {
        var vendor = await CreateVendorAsync("Deprecation Filter Vendor");
        var activeItem = await AddCatalogItemAsync(vendor.Id, "Keep This One");
        var toDeprecate = await AddCatalogItemAsync(vendor.Id, "Remove This One");

        await fixture.Host.Scenario(api =>
        {
            api.WithClaim(new Claim(ClaimTypes.Role, "SoftwareCenter"));
            api.Delete.Url($"/vendors/{vendor.Id}/catalog-items/{toDeprecate.Id}");
            api.StatusCodeShouldBe(204);
        });

        var response = await fixture.Host.Scenario(api =>
        {
            api.Get.Url($"/vendors/{vendor.Id}/catalog-items");
            api.StatusCodeShouldBeOk();
        });
        var items = response.ReadAsJson<CatalogCreateModel[]>()!;
        Assert.Contains(items, i => i.Id == activeItem.Id);
        Assert.DoesNotContain(items, i => i.Id == toDeprecate.Id);
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
