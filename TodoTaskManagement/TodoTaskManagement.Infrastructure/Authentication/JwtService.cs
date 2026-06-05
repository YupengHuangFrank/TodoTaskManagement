using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using TodoTaskManagement.Domain.Authentication;

namespace TodoTaskManagement.Infrastructure.Authentication;

public interface IJwtService
{
    OAuthToken GenerateAccessToken(Guid userId);
    OAuthToken GenerateRefreshToken(Guid userId);
    Guid? GetUserIdFromRefreshToken(string token);
}

public class JwtService : IJwtService
{
    private readonly string _secret;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _accessTokenExpiryMinutes;
    private readonly int _refreshTokenExpiryDays;

    public JwtService(IConfiguration config)
    {
        _secret = config["Jwt:Secret"]!;
        _issuer = config["Jwt:Issuer"]!;
        _audience = config["Jwt:Audience"]!;
        _accessTokenExpiryMinutes = int.Parse(config["Jwt:AccessTokenExpiryMinutes"]!);
        _refreshTokenExpiryDays = int.Parse(config["Jwt:RefreshTokenExpiryDays"]!);
    }

    public OAuthToken GenerateAccessToken(Guid userId) =>
        GenerateToken(userId, "access", TimeSpan.FromMinutes(_accessTokenExpiryMinutes));

    public OAuthToken GenerateRefreshToken(Guid userId) =>
        GenerateToken(userId, "refresh", TimeSpan.FromDays(_refreshTokenExpiryDays));

    public Guid? GetUserIdFromRefreshToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));

        try
        {
            handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out var validatedToken);

            var jwt = (JwtSecurityToken)validatedToken;

            if (jwt.Claims.FirstOrDefault(c => c.Type == "token_type")?.Value != "refresh")
                return null;

            var sub = jwt.Subject;
            return Guid.TryParse(sub, out var id) ? id : null;
        }
        catch
        {
            return null;
        }
    }

    private OAuthToken GenerateToken(Guid userId, string tokenType, TimeSpan lifetime)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiry = DateTime.UtcNow.Add(lifetime);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim("token_type", tokenType),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: expiry,
            signingCredentials: credentials
        );

        return new OAuthToken
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAt = expiry
        };
    }
}
