using Alba;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Software.Api.Vendors.Data;
using Software.Api.Vendors.Models;
using Software.Tests.Fixtures;
using SoftwareShared.Notifications;
using System.Security.Claims;


namespace Software.Tests.Vendors;

// any test classes we have that have this same attribute will use the same instance of the API and database.
// If we need "clean" database, create a different collection.
[Collection("SoftwareSystemTestCollection")]
public class AddingAVendor(SoftwareSystemTestFixture fixture) : IClassFixture<SoftwareSystemTestFixture>
{

    [Fact]
    public async Task NotAuthorized()
    {
        var vendorToPost = new CreateVendorRequestModel
        {
            Name = "Test Vendor",
            Url = "http://testvendor.com",
            PointOfContact = new VendorPointOfContactModel { Name = "John Doe", Email = "joe@aol.com", Phone = "867-5309" }
        };
        var response = await fixture.Host.Scenario(api =>
        {
            api.Post.Json(vendorToPost).ToUrl("/vendors");
            api.StatusCodeShouldBe(403);
        });

    }

    [Fact]
    public async Task RequestsAreValidated()
    {
        var vendorToPost = new CreateVendorRequestModel
        {
            Name = "",
            Url = "http://testvendor.com",
            PointOfContact = new VendorPointOfContactModel { Name = "", Email = "", Phone = "" }
        };
        var response = await fixture.Host.Scenario(api =>
        {

            api.WithClaim(new Claim(ClaimTypes.Role, "Manager"));
            api.WithClaim(new Claim(ClaimTypes.Role, "SoftwareCenter"));
            api.Post.Json(vendorToPost).ToUrl("/vendors");
            api.StatusCodeShouldBe(400);
        });

    }

    [Fact]
    public async Task CanAddVendor()
    {
        var vendorToPost = new CreateVendorRequestModel { 
            Name = "Test Vendor", 
            Url = "http://testvendor.com", 
            PointOfContact = new VendorPointOfContactModel { Name = "John Doe", Email = "joe@aol.com", Phone = "867-5309" } 
        };
        var response = await fixture.Host.Scenario(api =>
        {
            
            api.WithClaim(new Claim(ClaimTypes.Role, "Manager"));
            api.WithClaim(new Claim(ClaimTypes.Role, "SoftwareCenter"));
            api.Post.Json(vendorToPost).ToUrl("/vendors");
            api.StatusCodeShouldBe(201);
        });

        var vendorAdded = response.ReadAsJson<VendorDetailsModel>();
        Assert.NotNull(vendorAdded);

        var location = response.Context.Response.Headers.Location.ToString();
            Assert.NotNull(location);   


        var getResponse = await fixture.Host.Scenario(api =>
        {
            api.Get.Url(location);
            api.StatusCodeShouldBe(200);
        });

        var getBody = getResponse.ReadAsJson<VendorDetailsModel>();

        Assert.Equal(vendorAdded, getBody);

        using var scope = fixture.Host.Services.CreateScope();
        var session = scope.ServiceProvider.GetRequiredService<IDocumentSession>();

        var vendor = await session.Query<VendorEntity>().FirstOrDefaultAsync(v => v.Id == vendorAdded.Id, TestContext.Current.CancellationToken);
        Assert.NotNull(vendor);
        Assert.Equal(vendor.CreatedAt, fixture.TestClock);
        await fixture.NotificationMock.Received().SendNotification(Arg.Is<NotificationRequest>(n => n.Message.Contains("New vendor added") && n.Message.Contains(vendorToPost.Name)));

    }
}
