using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using NUnit.Framework;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TodoTaskManagement.Domain.Users;
using TodoTaskManagement.Infrastructure.Authentication;

namespace TodoTaskManagement.Tests.Infrastructure;

[TestFixture]
public class JwtServiceTests
{
    private const string Secret = "super-secret-test-key-that-is-at-least-32-chars!";
    private const string Issuer = "test-issuer";
    private const string Audience = "test-audience";

    private JwtService _jwtService = null!;

    [SetUp]
    public void SetUp()
    {
        var config = Substitute.For<IConfiguration>();
        config["Jwt:Secret"].Returns(Secret);
        config["Jwt:Issuer"].Returns(Issuer);
        config["Jwt:Audience"].Returns(Audience);
        config["Jwt:AccessTokenExpiryMinutes"].Returns("60");
        config["Jwt:RefreshTokenExpiryDays"].Returns("30");
        _jwtService = new JwtService(config);
    }

    // --- GenerateAccessToken ---
    [Test]
    public void GenerateAccessToken__ReturnsTokenWithExpiryApproximately60MinutesFromNow()
    {
        var before = DateTime.UtcNow.AddMinutes(59);
        var userId = Guid.NewGuid();
        var token = _jwtService.GenerateAccessToken(userId);
        var after = DateTime.UtcNow.AddMinutes(61);
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token.Token);
        var tokenType = jwt.Claims.FirstOrDefault(c => c.Type == "token_type")?.Value;

        Assert.That(token.Token, Is.Not.Null.And.Not.Empty);
        Assert.That(token.ExpiresAt, Is.GreaterThan(before).And.LessThan(after));
        Assert.That(jwt.Subject, Is.EqualTo(userId.ToString()));
        Assert.That(jwt.Subject, Is.EqualTo(userId.ToString()));
        Assert.That(tokenType, Is.EqualTo("access"));
    }

    // --- GenerateRefreshToken ---

    [Test]
    public void GenerateRefreshToken_ReturnsNonEmptyToken()
    {
        var before = DateTime.UtcNow.AddDays(29);
        var token = _jwtService.GenerateRefreshToken(Guid.NewGuid());
        var after = DateTime.UtcNow.AddDays(31);
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token.Token);

        var tokenType = jwt.Claims.FirstOrDefault(c => c.Type == "token_type")?.Value;

        Assert.That(token.Token, Is.Not.Null.And.Not.Empty);
        Assert.That(token.ExpiresAt, Is.GreaterThan(before).And.LessThan(after));
        Assert.That(tokenType, Is.EqualTo("refresh"));
    }

    // --- GetUserIdFromRefreshToken ---

    [Test]
    public void GetUserIdFromRefreshToken_ValidRefreshToken_ReturnsUserId()
    {
        var userId = Guid.NewGuid();
        var token = _jwtService.GenerateRefreshToken(userId);

        var result = _jwtService.GetUserIdFromRefreshToken(token.Token);

        Assert.That(result, Is.EqualTo(userId));
    }

    [Test]
    public void GetUserIdFromRefreshToken_AccessToken_ReturnsNull()
    {
        var userId = Guid.NewGuid();
        var accessToken = _jwtService.GenerateAccessToken(userId);

        var result = _jwtService.GetUserIdFromRefreshToken(accessToken.Token);

        Assert.That(result, Is.Null);
    }

    [Test]
    public void GetUserIdFromRefreshToken_InvalidToken_ReturnsNull()
    {
        var result = _jwtService.GetUserIdFromRefreshToken("not.a.valid.jwt");

        Assert.That(result, Is.Null);
    }

    [Test]
    public void GetUserIdFromRefreshToken_ExpiredToken_ReturnsNull()
    {
        var userId = Guid.NewGuid();
        var expiredToken = BuildExpiredRefreshToken(userId);

        var result = _jwtService.GetUserIdFromRefreshToken(expiredToken);

        Assert.That(result, Is.Null);
    }

    [Test]
    public void GetUserIdFromRefreshToken_TokenSignedWithDifferentSecret_ReturnsNull()
    {
        var userId = Guid.NewGuid();
        var wrongKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("completely-different-secret-key-123456!"));
        var creds = new SigningCredentials(wrongKey, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim("token_type", "refresh")
        };
        var jwt = new JwtSecurityToken(Issuer, Audience, claims,
            expires: DateTime.UtcNow.AddDays(30), signingCredentials: creds);
        var token = new JwtSecurityTokenHandler().WriteToken(jwt);

        var result = _jwtService.GetUserIdFromRefreshToken(token);

        Assert.That(result, Is.Null);
    }

    private static string BuildExpiredRefreshToken(Guid userId)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim("token_type", "refresh"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        var jwt = new JwtSecurityToken(Issuer, Audience, claims,
            expires: DateTime.UtcNow.AddSeconds(-1), signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }
}
