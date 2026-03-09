using System.ComponentModel.DataAnnotations;

namespace Software.Api.Vendors.Models;

public record VendorPointOfContactModel
{
    [MinLength(3), MaxLength(100)]
    public required string Name { get; set; }
    [EmailAddress]
    public required string Email { get; set; }
    
    public required string Phone { get; set; }
}
