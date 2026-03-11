using JasperFx.Events;
using System.ComponentModel.DataAnnotations;

namespace Vendors.Api.Vendors;

public record VendorDetailsReadModel
{
    public required Guid Id { get; set; }

    public int Version { get; set; }

    public DateTimeOffset Created { get; set; }

    public required string Name { get; set; }
    
    public required string Url { get; set; }
    
    public VendorDetailsPoc PointOfContact { get; set; } = new VendorDetailsPoc();

    public static VendorDetailsReadModel Create(IEvent<VendorCreated> evt)
    {
        return new VendorDetailsReadModel { Id = evt.StreamId, Created = evt.Timestamp, Name = evt.Data.Name, Url = evt.Data.Url };
    }

    public static VendorDetailsReadModel Apply(PointOfContactAssignedToAVendor evt, VendorDetailsReadModel current)
    {
        return current with { PointOfContact = new VendorDetailsPoc { Name = evt.Name, Email = evt.Email, Phone = evt.Phone } };
    }
}

public record VendorDetailsPoc
{
    public string Name { get; set; }
    
    public string Email { get; set; }
    
    public string Phone { get; set; }
}

public record VendorListReadModel
{

}