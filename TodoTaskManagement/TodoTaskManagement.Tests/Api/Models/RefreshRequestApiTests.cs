using System.ComponentModel.DataAnnotations;
using NUnit.Framework;
using TodoTaskManagement.Models;

namespace TodoTaskManagement.Tests.Api.Models;

[TestFixture]
public class RefreshRequestApiTests
{
    private static IList<ValidationResult> Validate(RefreshRequestApi model)
    {
        var context = new ValidationContext(model);
        return model.Validate(context).ToList();
    }

    [Test]
    public void Validate_ValidToken_ReturnsNoErrors()
    {
        var model = new RefreshRequestApi { RefreshToken = "some.jwt.token" };

        var results = Validate(model);

        Assert.That(results, Is.Empty);
    }

    [Test]
    public void Validate_NullToken_ReturnsError()
    {
        var model = new RefreshRequestApi { RefreshToken = null };

        var results = Validate(model);

        Assert.That(results, Has.Count.EqualTo(1));
        Assert.That(results[0].MemberNames, Contains.Item("RefreshToken"));
    }

    [Test]
    public void Validate_WhitespaceToken_ReturnsError()
    {
        var model = new RefreshRequestApi { RefreshToken = "   " };

        var results = Validate(model);

        Assert.That(results, Has.Count.EqualTo(1));
        Assert.That(results[0].MemberNames, Contains.Item("RefreshToken"));
    }
}
