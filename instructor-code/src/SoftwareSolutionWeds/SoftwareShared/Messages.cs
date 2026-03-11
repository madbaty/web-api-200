
namespace SoftwareShared.Messages;

public record Vendor
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public record VendorTombStone
{
    public Guid Id { get; set; }
}