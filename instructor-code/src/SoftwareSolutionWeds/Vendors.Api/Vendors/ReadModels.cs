
using JasperFx.Events;
using Marten.Events.Aggregation;
using Marten.Events.Projections;


namespace Vendors.Api.Vendors;


public record VendorDetailsReadModel
{
 
    public required Guid Id { get; set; }

    public int Version { get; set; }

    public DateTimeOffset Created { get; set; }
  
    public required string Name { get; set; }

    public DateTimeOffset? DeactivatedOn { get; set; }

    public required string Url { get; set; }
    public VendorDetailsPoc PointOfContact { get; set; } = new VendorDetailsPoc();

    public static VendorDetailsReadModel Create(IEvent<VendorCreated> evt)
    {
        return new VendorDetailsReadModel { Id = evt.StreamId, Created = evt.Timestamp, Name = evt.Data.Name, Url = evt.Data.Url };
    }

    public static void Apply(PointOfContactAssignedToAVendor evt, VendorDetailsReadModel current)
    {
        current.PointOfContact = new VendorDetailsPoc { Name = evt.Name, Email = evt.Email, Phone = evt.Phone };
    }

    public static void Apply(IEvent<VendorDeleted> evt, VendorDetailsReadModel current)
    {
        current.DeactivatedOn = evt.Timestamp;

    }
   
}
public record VendorDetailsPoc
{
   
    public  string Name { get; set; }
   
    public  string Email { get; set; }

    public  string Phone { get; set; }
}



public class VendorListReadModel
{
    public Guid Id { get; set; }
    public required string Name { get; set; }

    public required string Url { get; set; }
}

public class VendorListProjection : SingleStreamProjection<VendorListReadModel, Guid>
{

    public VendorListProjection()
    {
        // 
        DeleteEvent<VendorDeleted>(); // delete the row from the table upon this event.
    }
    public static VendorListReadModel Create(IEvent<VendorCreated> evt)
    {
        return new VendorListReadModel { Id = evt.StreamId, Name = evt.Data.Name, Url = evt.Data.Url };
    }
};