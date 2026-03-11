using System.ComponentModel.DataAnnotations;

namespace Software.Api.Vendors.Models;

public record CreateVendorRequestModel
{
    // we are doing all this to make sure you have a good request before we do any work.
    [MinLength(3), MaxLength(100), Required]
    public required string Name { get; set; }
   
    public required string Url { get; set; }
    public required VendorPointOfContactModel PointOfContact { get; set; } 
}
/*{
    "name": "Microsoft",
    "url": "https://www.microsoft.com",
    "pointOfContact": {
        "name": "Satya Nadella",
        "email": "satya@microsoft",
        "phone": "888 999-1212"
    }
}*/