using NUnit.Framework;
using TodoTaskManagement.Infrastructure.Authentication;

namespace TodoTaskManagement.Tests.Infrastructure;

[TestFixture]
public class PasswordServiceTests
{
    private PasswordService _passwordService = null!;

    [SetUp]
    public void SetUp()
    {
        _passwordService = new PasswordService();
    }

    [Test]
    public void Hash_ReturnsNonEmptyString()
    {
        const string password = "mypassword";
        var hash = _passwordService.Hash(password);

        Assert.That(hash, Is.Not.Null.And.Not.Empty);
        Assert.That(hash, Is.Not.EqualTo(password));
    }

    [Test]
    public void Hash_ReturnsDifferentHashForSamePasswordEachCall()
    {
        var hash1 = _passwordService.Hash("mypassword");
        var hash2 = _passwordService.Hash("mypassword");

        // BCrypt uses a random salt each time
        Assert.That(hash1, Is.Not.EqualTo(hash2));
    }

    [Test]
    public void Verify_CorrectPassword_ReturnsTrue()
    {
        const string password = "mypassword";
        var hash = _passwordService.Hash(password);

        var result = _passwordService.Verify(password, hash);

        Assert.That(result, Is.True);
    }

    [Test]
    public void Verify_WrongPassword_ReturnsFalse()
    {
        var hash = _passwordService.Hash("correctpassword");

        var result = _passwordService.Verify("wrongpassword", hash);

        Assert.That(result, Is.False);
    }

    [Test]
    public void Verify_EmptyPasswordAgainstHash_ReturnsFalse()
    {
        var hash = _passwordService.Hash("somepassword");

        var result = _passwordService.Verify("", hash);

        Assert.That(result, Is.False);
    }
}
