namespace TodoTaskManagement.Domain.Authentication;

public class OAuthToken
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}
