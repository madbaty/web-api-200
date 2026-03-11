namespace Software.Api.Vendors;

public class BlockedVendorsOptions
{
    public static string SectionName = "BlockedVendors";

    public List<string> BlockedNames { get; set; } = [];
}
