using Alba;
using Software.Api.CatalogItems.Operations;
using Software.Api.Vendors.Models;
using Software.Tests.Fixtures;
using System.Security.Claims;

namespace Software.Tests.CatalogItems;

[Collection("SoftwareSystemTestCollection")]
public class AddingACatalogItem(SoftwareSystemTestFixture fixture) : IClassFixture<SoftwareSystemTestFixture>
{
    [Fact]
    public async Task CanAddCatalogItem()
    {
        var vendor = await CreateVendorAsync("Catalog Item Vendor");

        var itemRequest = new CatalogCreateModel { Name = "Awesome Software" };
        var response = await fixture.Host.Scenario(api =>
        {
            api.WithClaim(new Claim(ClaimTypes.Role, "SoftwareCenter"));
            api.Post.Json(itemRequest).ToUrl($"/vendors/{vendor.Id}/catalog-items");
            api.StatusCodeShouldBe(201);
        });

        var item = response.ReadAsJson<CatalogCreateModel>();
        Assert.NotNull(item);
        Assert.NotEqual(Guid.Empty, item.Id);
        Assert.Equal(vendor.Id, item.VendorId);
        Assert.Equal("Awesome Software", item.Name);
        Assert.False(item.IsDeprecated);
    }

    [Fact]
    public async Task ReturnsNotFoundForUnknownVendor()
    {
        var itemRequest = new CatalogCreateModel { Name = "Orphan Software" };
        await fixture.Host.Scenario(api =>
        {
            api.WithClaim(new Claim(ClaimTypes.Role, "SoftwareCenter"));
            api.Post.Json(itemRequest).ToUrl($"/vendors/{Guid.NewGuid()}/catalog-items");
            api.StatusCodeShouldBe(404);
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
}
