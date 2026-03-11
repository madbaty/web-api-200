using Software.Api.Vendors.Models;
using System.ComponentModel.DataAnnotations;

namespace Software.Tests.Vendors;

public class CreateVendorModelUnitTests
{
    private CreateVendorRequestModel _model = new CreateVendorRequestModel
    {
        Name = "Test Vendor",
        Url = "http://testvendor.com",
        PointOfContact = new VendorPointOfContactModel { Name = "John Doe", Email = "joe@aol.com", Phone = "867-5309" }
    };
    [Fact]
    public void ModelIsValid()
    {
        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(_model, new ValidationContext(_model), validationResults, true);
        Assert.True(isValid);
    }
     [Theory]
     [InlineData("ab")]
     [InlineData("")]
     [InlineData(" ")]
     public void NameMustBeAtLeast3Characters(string name)
     {

        var model = _model with { Name = name };
        var validationResults = new List<ValidationResult>();
         var isValid = Validator.TryValidateObject(model, new ValidationContext(model), validationResults, true);
         Assert.False(isValid);
         Assert.Contains(validationResults, v => v.MemberNames.Contains(nameof(CreateVendorRequestModel.Name)));
    }

}
