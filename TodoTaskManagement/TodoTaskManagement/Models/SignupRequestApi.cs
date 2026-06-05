using System.ComponentModel.DataAnnotations;
using System.Net.Mail;

namespace TodoTaskManagement.Models;

public class SignupRequestApi : IValidatableObject
{
    public string? Email { get; set; }
    public string? Password { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Email))
        {
            yield return new ValidationResult("Email is required.", [nameof(Email)]);
        }
        else if (!IsValidEmail(Email))
        {
            yield return new ValidationResult("Email format is invalid.", [nameof(Email)]);
        }

        if (string.IsNullOrWhiteSpace(Password))
            yield return new ValidationResult("Password is required.", [nameof(Password)]);
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            _ = new MailAddress(email);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
