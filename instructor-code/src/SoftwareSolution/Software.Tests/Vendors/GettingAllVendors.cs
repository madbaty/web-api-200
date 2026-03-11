using Alba;
using Software.Api.Vendors.Models;
using Software.Tests.Fixtures;

namespace Software.Tests.Vendors;

[Collection("SoftwareSystemTestCollection")]
public class GettingAllVendors(SoftwareSystemTestFixture fixture) : IClassFixture<SoftwareSystemTestFixture>
{
    [Fact]
    public async Task CanGetAllVendors()
    {
        var response = await fixture.Host.Scenario(api =>
        {
            api.Get.Url("/vendors");
            api.StatusCodeShouldBeOk();
        });

        var vendors = response.ReadAsJson<VendorSummaryModel[]>();
        Assert.NotNull(vendors);
    }
}
