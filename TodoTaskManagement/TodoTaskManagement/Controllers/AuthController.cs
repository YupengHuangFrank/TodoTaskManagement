using Microsoft.AspNetCore.Mvc;
using TodoTaskManagement.Application.Authentication;
using TodoTaskManagement.Domain.Authentication;
using TodoTaskManagement.Models;

namespace TodoTaskManagement.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService, IWebHostEnvironment env) : ControllerBase
{
    [HttpPost("signup")]
    public async Task<IActionResult> Signup([FromBody] SignupRequestApi body)
    {
        try
        {
            var tokens = await authService.SignupAsync(body.Email!, body.Password!);
            SetTokenCookies(tokens);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestApi body)
    {
        var tokens = await authService.LoginAsync(body.Email!, body.Password!);
        SetTokenCookies(tokens);
        return NoContent();
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        if (!Request.Cookies.TryGetValue("refresh_token", out var refreshToken))
            return Unauthorized(new { error = "Refresh token cookie is missing." });

        var tokens = await authService.RefreshAsync(refreshToken);
        SetTokenCookies(tokens);
        return NoContent();
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("access_token");
        Response.Cookies.Delete("refresh_token");
        return NoContent();
    }

    private void SetTokenCookies(UserTokens tokens)
    {
        var isProduction = !env.IsDevelopment();

        Response.Cookies.Append("access_token", tokens.AccessToken.Token, new CookieOptions
        {
            HttpOnly = true,
            Secure = isProduction,
            SameSite = SameSiteMode.Strict,
            Expires = tokens.AccessToken.ExpiresAt
        });

        Response.Cookies.Append("refresh_token", tokens.RefreshToken.Token, new CookieOptions
        {
            HttpOnly = true,
            Secure = isProduction,
            SameSite = SameSiteMode.Strict,
            Expires = tokens.RefreshToken.ExpiresAt
        });
    }
}
