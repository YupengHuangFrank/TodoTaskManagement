using System.ComponentModel.DataAnnotations;

namespace TodoTaskManagement.Models;

public class RefreshRequestApi : IValidatableObject
{
    public string? RefreshToken { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(RefreshToken))
            yield return new ValidationResult("RefreshToken is required.", [nameof(RefreshToken)]);
    }
}
