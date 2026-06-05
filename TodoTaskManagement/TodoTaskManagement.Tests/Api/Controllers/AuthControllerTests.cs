using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using TodoTaskManagement.Application.Authentication;
using TodoTaskManagement.Controllers;
using TodoTaskManagement.Domain.Authentication;
using TodoTaskManagement.Models;

namespace TodoTaskManagement.Tests.Api.Controllers;

[TestFixture]
public class AuthControllerTests
{
    private IAuthService _authService = null!;
    private IWebHostEnvironment _env = null!;

    [SetUp]
    public void SetUp()
    {
        _authService = Substitute.For<IAuthService>();
        _env = Substitute.For<IWebHostEnvironment>();
        _env.EnvironmentName.Returns("Development");
    }

    private AuthController CreateController()
    {
        var controller = new AuthController(_authService, _env);
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        return controller;
    }

    private static UserTokens MakeTokens() => new()
    {
        AccessToken = new OAuthToken { Token = "access.token", ExpiresAt = DateTime.UtcNow.AddMinutes(60) },
        RefreshToken = new OAuthToken { Token = "refresh.token", ExpiresAt = DateTime.UtcNow.AddDays(30) }
    };

    // --- Signup ---

    [Test]
    public async Task Signup_ValidRequest_Returns204()
    {
        _authService.SignupAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(MakeTokens());

        var result = await CreateController().Signup(new SignupRequestApi { Email = "new@example.com", Password = "pass" });

        Assert.That(result, Is.InstanceOf<NoContentResult>());
        await _authService.Received(1).SignupAsync("new@example.com", "pass");
    }

    [Test]
    public async Task Signup_DuplicateEmail_Returns409()
    {
        _authService.SignupAsync(Arg.Any<string>(), Arg.Any<string>())
            .Throws(new InvalidOperationException("Email is already in use."));

        var result = (ConflictObjectResult)await CreateController()
            .Signup(new SignupRequestApi { Email = "dup@example.com", Password = "pass" });

        Assert.That(result, Is.InstanceOf<ConflictObjectResult>());
        Assert.That(result.Value?.ToString(), Does.Contain("already in use"));
    }

    // --- Login ---

    [Test]
    public async Task Login_ValidCredentials_Returns204()
    {
        _authService.LoginAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(MakeTokens());

        var result = await CreateController().Login(new LoginRequestApi { Email = "user@example.com", Password = "pass" });

        Assert.That(result, Is.InstanceOf<NoContentResult>());
        await _authService.Received(1).LoginAsync("user@example.com", "pass");
    }

    [Test]
    public void Login_InvalidCredentials_PropagatesUnauthorizedAccessException()
    {
        _authService.LoginAsync(Arg.Any<string>(), Arg.Any<string>())
            .Throws(new UnauthorizedAccessException("Invalid credentials."));

        Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await CreateController().Login(new LoginRequestApi { Email = "x@x.com", Password = "wrong" }));
    }

    // --- Refresh ---

    [Test]
    public async Task Refresh_MissingCookie_Returns401()
    {
        var result = await CreateController().Refresh();

        Assert.That(result, Is.InstanceOf<UnauthorizedObjectResult>());
    }

    [Test]
    public async Task Refresh_ValidCookie_Returns204()
    {
        _authService.RefreshAsync(Arg.Any<string>()).Returns(MakeTokens());

        var controller = CreateController();
        controller.ControllerContext.HttpContext.Request.Headers["Cookie"] = "refresh_token=some.refresh.token";

        var result = await controller.Refresh();

        Assert.That(result, Is.InstanceOf<NoContentResult>());
        await _authService.Received(1).RefreshAsync("some.refresh.token");
    }

    // --- Logout ---

    [Test]
    public void Logout_AlwaysReturns204()
    {
        var result = CreateController().Logout();

        Assert.That(result, Is.InstanceOf<NoContentResult>());
    }
}
