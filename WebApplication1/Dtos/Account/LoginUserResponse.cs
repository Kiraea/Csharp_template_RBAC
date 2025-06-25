namespace WebApplication1.Dtos.Account;

public class LoginUserResponse
{
    
    public required string Email { get; set; }
    public required string token { get; set; }
}