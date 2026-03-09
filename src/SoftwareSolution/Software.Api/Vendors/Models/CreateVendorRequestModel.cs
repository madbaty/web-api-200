using System.ComponentModel.DataAnnotations;

namespace Software.Api.Vendors.Models;

public record CreateVendorRequestModel
{
    [MinLength(3), MaxLength(100)]
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