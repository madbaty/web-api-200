namespace Software.Api.Vendors.Models;

public record VendorSummaryModel
{
    public Guid Id { get; set; }
    public required string Name { get; set; }

    public required string Url { get; set; }
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