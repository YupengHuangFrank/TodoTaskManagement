using NSubstitute;
using NUnit.Framework;
using TodoTaskManagement.Application.Authentication;
using TodoTaskManagement.Domain.Authentication;
using TodoTaskManagement.Domain.Users;
using TodoTaskManagement.Infrastructure.Authentication;
using TodoTaskManagement.Infrastructure.Users;

namespace TodoTaskManagement.Tests.Application.Authentication;

[TestFixture]
public class AuthServiceTests
{
    private IUserRepository _userRepository = null!;
    private IJwtService _jwtService = null!;
    private IPasswordService _passwordService = null!;
    private AuthService _authService = null!;

    [SetUp]
    public void SetUp()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _jwtService = Substitute.For<IJwtService>();
        _passwordService = Substitute.For<IPasswordService>();
        _authService = new AuthService(_userRepository, _jwtService, _passwordService);
    }

    private static UserTokens MakeTokens() => new()
    {
        AccessToken = new OAuthToken { Token = "access", ExpiresAt = DateTime.UtcNow.AddMinutes(60) },
        RefreshToken = new OAuthToken { Token = "refresh", ExpiresAt = DateTime.UtcNow.AddDays(30) }
    };

    // --- SignupAsync ---

    [Test]
    public async Task SignupAsync_NewEmail_AddsUserAndReturnsTokens()
    {
        const string email = "new@example.com";
        const string password = "password";
        var userId = Guid.NewGuid();
        var expectedTokens = MakeTokens();

        _userRepository.GetByEmailAsync(email).Returns((User?)null);
        _passwordService.Hash(password).Returns("hashed");
        _jwtService.GenerateAccessToken(Arg.Any<Guid>()).Returns(expectedTokens.AccessToken);
        _jwtService.GenerateRefreshToken(Arg.Any<Guid>()).Returns(expectedTokens.RefreshToken);

        var tokens = await _authService.SignupAsync(email, password);

        await _userRepository.Received(1).AddAsync(Arg.Is<User>(u => u.Email == email && u.PasswordHash == "hashed"));
        Assert.That(tokens.AccessToken.Token, Is.EqualTo(expectedTokens.AccessToken.Token));
        Assert.That(tokens.RefreshToken.Token, Is.EqualTo(expectedTokens.RefreshToken.Token));
    }

    [Test]
    public async Task SignupAsync_ExistingEmail_ThrowsInvalidOperationException()
    {
        const string email = "existing@example.com";
        var existingUser = new User { Id = Guid.NewGuid(), Email = email, PasswordHash = "hash" };

        _userRepository.GetByEmailAsync(email).Returns(existingUser);

        Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _authService.SignupAsync(email, "password"));

        await _userRepository.DidNotReceive().AddAsync(Arg.Any<User>());
    }

    // --- LoginAsync ---

    [Test]
    public async Task LoginAsync_ValidCredentials_ReturnsTokens()
    {
        const string email = "user@example.com";
        const string password = "password";
        var user = new User { Id = Guid.NewGuid(), Email = email, PasswordHash = "hash" };
        var expectedTokens = MakeTokens();

        _userRepository.GetByEmailAsync(email).Returns(user);
        _passwordService.Verify(password, "hash").Returns(true);
        _jwtService.GenerateAccessToken(user.Id).Returns(expectedTokens.AccessToken);
        _jwtService.GenerateRefreshToken(user.Id).Returns(expectedTokens.RefreshToken);

        var tokens = await _authService.LoginAsync(email, password);

        Assert.That(tokens.AccessToken.Token, Is.EqualTo(expectedTokens.AccessToken.Token));
    }

    [Test]
    public void LoginAsync_UnknownEmail_ThrowsUnauthorizedAccessException()
    {
        _userRepository.GetByEmailAsync(Arg.Any<string>()).Returns((User?)null);

        Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await _authService.LoginAsync("unknown@example.com", "password"));
    }

    [Test]
    public void LoginAsync_WrongPassword_ThrowsUnauthorizedAccessException()
    {
        var user = new User { Id = Guid.NewGuid(), Email = "user@example.com", PasswordHash = "hash" };

        _userRepository.GetByEmailAsync("user@example.com").Returns(user);
        _passwordService.Verify("wrong", "hash").Returns(false);

        Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await _authService.LoginAsync("user@example.com", "wrong"));
    }

    // --- RefreshAsync ---

    [Test]
    public async Task RefreshAsync_ValidToken_ReturnsNewTokens()
    {
        var userId = Guid.NewGuid();
        const string refreshToken = "valid.refresh.token";
        var user = new User { Id = userId, Email = "user@example.com", PasswordHash = "hash" };
        var expectedTokens = MakeTokens();

        _jwtService.GetUserIdFromRefreshToken(refreshToken).Returns(userId);
        _userRepository.GetByIdAsync(userId).Returns(user);
        _jwtService.GenerateAccessToken(userId).Returns(expectedTokens.AccessToken);
        _jwtService.GenerateRefreshToken(userId).Returns(expectedTokens.RefreshToken);

        var tokens = await _authService.RefreshAsync(refreshToken);

        Assert.That(tokens.AccessToken.Token, Is.EqualTo(expectedTokens.AccessToken.Token));
    }

    [Test]
    public void RefreshAsync_InvalidToken_ThrowsUnauthorizedAccessException()
    {
        _jwtService.GetUserIdFromRefreshToken(Arg.Any<string>()).Returns((Guid?)null);

        Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await _authService.RefreshAsync("invalid.token"));
    }

    [Test]
    public void RefreshAsync_UserNoLongerExists_ThrowsUnauthorizedAccessException()
    {
        var userId = Guid.NewGuid();

        _jwtService.GetUserIdFromRefreshToken(Arg.Any<string>()).Returns(userId);
        _userRepository.GetByIdAsync(userId).Returns((User?)null);

        Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await _authService.RefreshAsync("valid.token"));
    }
}
