using System.ComponentModel.DataAnnotations;

namespace Vendors.Api.Vendors;

public record CreateVendorRequestModel
{
    // we are doing all this to make sure you have a good request before we do any work.
    [MinLength(3), MaxLength(100), Required]
    public required string Name { get; set; }

    public required string Url { get; set; }
    public required VendorPointOfContactModel PointOfContact { get; set; }
}
public record VendorPointOfContactModel
{
    [MinLength(3), MaxLength(100)]
    public required string Name { get; set; }
    [EmailAddress]
    public required string Email { get; set; }

    public required string Phone { get; set; }
}

public record VendorSummaryModel
{
    public Guid Id { get; set; }
    public required string Name { get; set; }

    public required string Url { get; set; }
}