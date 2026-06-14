using System.ComponentModel.DataAnnotations;

namespace Back.Domain.UnitTests;

public static class ValidationHelper
{
    public static IList<ValidationResult> ValidateObject(object instance)
    {
        var results = new List<ValidationResult>();
        var context = new ValidationContext(instance);
        Validator.TryValidateObject(instance, context, results, validateAllProperties: true);
        return results;
    }
}
