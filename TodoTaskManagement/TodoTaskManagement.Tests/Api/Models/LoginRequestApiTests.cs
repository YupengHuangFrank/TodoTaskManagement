using System.ComponentModel.DataAnnotations;
using NUnit.Framework;
using TodoTaskManagement.Models;

namespace TodoTaskManagement.Tests.Api.Models;

[TestFixture]
public class LoginRequestApiTests
{
    private static IList<ValidationResult> Validate(LoginRequestApi model)
    {
        var context = new ValidationContext(model);
        return model.Validate(context).ToList();
    }

    [Test]
    public void Validate_ValidCredentials_ReturnsNoErrors()
    {
        var model = new LoginRequestApi { Email = "user@example.com", Password = "secret" };

        var results = Validate(model);

        Assert.That(results, Is.Empty);
    }

    [Test]
    public void Validate_NullEmail_ReturnsEmailError()
    {
        var model = new LoginRequestApi { Email = null, Password = "secret" };

        var results = Validate(model);

        Assert.That(results, Has.Count.EqualTo(1));
        Assert.That(results[0].MemberNames, Contains.Item("Email"));
    }

    [Test]
    public void Validate_WhitespaceEmail_ReturnsEmailError()
    {
        var model = new LoginRequestApi { Email = "  ", Password = "secret" };

        var results = Validate(model);

        Assert.That(results, Has.Count.EqualTo(1));
        Assert.That(results[0].MemberNames, Contains.Item("Email"));
    }

    [Test]
    public void Validate_NullPassword_ReturnsPasswordError()
    {
        var model = new LoginRequestApi { Email = "user@example.com", Password = null };

        var results = Validate(model);

        Assert.That(results, Has.Count.EqualTo(1));
        Assert.That(results[0].MemberNames, Contains.Item("Password"));
    }

    [Test]
    public void Validate_WhitespacePassword_ReturnsPasswordError()
    {
        var model = new LoginRequestApi { Email = "user@example.com", Password = "" };

        var results = Validate(model);

        Assert.That(results, Has.Count.EqualTo(1));
        Assert.That(results[0].MemberNames, Contains.Item("Password"));
    }

    [Test]
    public void Validate_BothFieldsMissing_ReturnsTwoErrors()
    {
        var model = new LoginRequestApi { Email = null, Password = null };

        var results = Validate(model);

        Assert.That(results, Has.Count.EqualTo(2));
    }
}
