using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Database.Entities;

public static class EntityValidator
{
    public static void Validate(object obj) {
        var context = new ValidationContext(obj, serviceProvider: null, items: null);
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(obj, context, results, true);

        if (isValid) return;
        
        var sbrErrors = new StringBuilder();
        foreach (var validationResult in results) {
            sbrErrors.AppendLine(validationResult.ErrorMessage);
        }
        throw new ValidationException(sbrErrors.ToString());
    }
}