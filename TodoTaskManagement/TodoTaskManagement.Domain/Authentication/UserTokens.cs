namespace TodoTaskManagement.Domain.Authentication;

public class UserTokens
{
    public OAuthToken AccessToken { get; set; } = null!;
    public OAuthToken RefreshToken { get; set; } = null!;
}
