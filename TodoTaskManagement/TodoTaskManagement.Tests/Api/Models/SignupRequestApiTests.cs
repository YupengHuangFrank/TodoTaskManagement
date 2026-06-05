using System.ComponentModel.DataAnnotations;
using NUnit.Framework;
using TodoTaskManagement.Models;

namespace TodoTaskManagement.Tests.Api.Models;

[TestFixture]
public class SignupRequestApiTests
{
    private static IList<ValidationResult> Validate(SignupRequestApi model)
    {
        var context = new ValidationContext(model);
        return model.Validate(context).ToList();
    }

    [Test]
    public void Validate_ValidEmailAndPassword_ReturnsNoErrors()
    {
        var model = new SignupRequestApi { Email = "user@example.com", Password = "secret" };

        var results = Validate(model);

        Assert.That(results, Is.Empty);
    }

    [Test]
    public void Validate_NullEmail_ReturnsEmailRequiredError()
    {
        var model = new SignupRequestApi { Email = null, Password = "secret" };

        var results = Validate(model);

        Assert.That(results, Has.Count.EqualTo(1));
        Assert.That(results[0].MemberNames, Contains.Item("Email"));
        Assert.That(results[0].ErrorMessage, Does.Contain("required"));
    }

    [Test]
    public void Validate_WhitespaceEmail_ReturnsEmailRequiredError()
    {
        var model = new SignupRequestApi { Email = "   ", Password = "secret" };

        var results = Validate(model);

        Assert.That(results, Has.Count.EqualTo(1));
        Assert.That(results[0].MemberNames, Contains.Item("Email"));
        Assert.That(results[0].ErrorMessage, Does.Contain("required"));
    }

    [Test]
    public void Validate_InvalidEmailFormat_ReturnsFormatError()
    {
        var model = new SignupRequestApi { Email = "not-an-email", Password = "secret" };

        var results = Validate(model);

        Assert.That(results, Has.Count.EqualTo(1));
        Assert.That(results[0].MemberNames, Contains.Item("Email"));
        Assert.That(results[0].ErrorMessage, Does.Contain("invalid"));
    }

    [Test]
    public void Validate_NullPassword_ReturnsPasswordRequiredError()
    {
        var model = new SignupRequestApi { Email = "user@example.com", Password = null };

        var results = Validate(model);

        Assert.That(results, Has.Count.EqualTo(1));
        Assert.That(results[0].MemberNames, Contains.Item("Password"));
    }

    [Test]
    public void Validate_WhitespacePassword_ReturnsPasswordRequiredError()
    {
        var model = new SignupRequestApi { Email = "user@example.com", Password = "   " };

        var results = Validate(model);

        Assert.That(results, Has.Count.EqualTo(1));
        Assert.That(results[0].MemberNames, Contains.Item("Password"));
    }

    [Test]
    public void Validate_BothFieldsMissing_ReturnsTwoErrors()
    {
        var model = new SignupRequestApi { Email = null, Password = null };

        var results = Validate(model);

        Assert.That(results, Has.Count.EqualTo(2));
    }
}
