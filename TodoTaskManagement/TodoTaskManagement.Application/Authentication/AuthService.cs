using TodoTaskManagement.Domain.Authentication;
using TodoTaskManagement.Domain.Users;
using TodoTaskManagement.Infrastructure.Authentication;
using TodoTaskManagement.Infrastructure.Users;

namespace TodoTaskManagement.Application.Authentication;

public interface IAuthService
{
    Task<UserTokens> SignupAsync(string email, string password);
    Task<UserTokens> LoginAsync(string email, string password);
    Task<UserTokens> RefreshAsync(string refreshToken);
}

public class AuthService(
    IUserRepository userRepository,
    IJwtService jwtService,
    IPasswordService passwordService) : IAuthService
{
    public async Task<UserTokens> SignupAsync(string email, string password)
    {
        var existing = await userRepository.GetByEmailAsync(email);
        if (existing is not null)
            throw new InvalidOperationException("Email is already in use.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = passwordService.Hash(password)
        };

        await userRepository.AddAsync(user);
        return IssueTokens(user.Id);
    }

    public async Task<UserTokens> LoginAsync(string email, string password)
    {
        var user = await userRepository.GetByEmailAsync(email);
        if (user is null || !passwordService.Verify(password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        return IssueTokens(user.Id);
    }

    public async Task<UserTokens> RefreshAsync(string refreshToken)
    {
        var userId = jwtService.GetUserIdFromRefreshToken(refreshToken);
        if (userId is null)
            throw new UnauthorizedAccessException("Invalid or expired refresh token.");

        var user = await userRepository.GetByIdAsync(userId.Value);
        if (user is null)
            throw new UnauthorizedAccessException("User no longer exists.");

        return IssueTokens(user.Id);
    }

    private UserTokens IssueTokens(Guid userId) => new()
    {
        AccessToken = jwtService.GenerateAccessToken(userId),
        RefreshToken = jwtService.GenerateRefreshToken(userId)
    };
}
