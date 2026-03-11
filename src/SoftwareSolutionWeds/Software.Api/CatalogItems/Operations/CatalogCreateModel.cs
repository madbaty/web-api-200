using System.ComponentModel.DataAnnotations;

namespace Software.Api.CatalogItems.Operations;

public record CatalogCreateModel
{
    public Guid Id { get; set; }
    public Guid VendorId { get; set; }

    [MinLength(5), MaxLength(100)]
    public required string Name { get; set; }

    public bool IsDeprecated { get; set; } = false;
}