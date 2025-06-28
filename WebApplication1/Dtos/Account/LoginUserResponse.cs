namespace WebApplication1.Dtos.Account;

public class LoginUserResponse
{
    
    public required string Email { get; set; }
    public required string AccessToken { get; set; }
    public required string RefreshToken { get; set; }
}