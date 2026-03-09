using Software.Api.Vendors.Models;

namespace Software.Api.Vendors.Data;

public class VendorEntity
{
    public Guid Id { get; set; }
    public required string Name { get; set; }

    public required string Url { get; set; }
    public required VendorPointOfContactModel PointOfContact { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public static VendorEntity From(CreateVendorRequestModel request, TimeProvider clock) =>
        new()
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Url = request.Url,
            PointOfContact = request.PointOfContact,
            CreatedAt = clock.GetUtcNow()
        };

    public VendorDetailsModel ToDetails() =>
        new()
        {
            Id = Id,
            Name = Name,
            Url = Url,
            PointOfContact = PointOfContact,
            CreatedAt = CreatedAt
        };

    public VendorSummaryModel ToSummary() =>
        new()
        {
            Id = Id,
            Name = Name,
            Url = Url
        };
}
