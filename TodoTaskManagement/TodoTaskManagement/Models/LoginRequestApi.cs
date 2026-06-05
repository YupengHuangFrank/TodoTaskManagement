using System.ComponentModel.DataAnnotations;

namespace TodoTaskManagement.Models;

public class LoginRequestApi : IValidatableObject
{
    public string? Email { get; set; }
    public string? Password { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Email))
            yield return new ValidationResult("Email is required.", [nameof(Email)]);

        if (string.IsNullOrWhiteSpace(Password))
            yield return new ValidationResult("Password is required.", [nameof(Password)]);
    }
}
