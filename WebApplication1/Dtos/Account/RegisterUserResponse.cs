namespace WebApplication1.Dtos.Account;

public class RegisterUserResponse
{
    public required string Email { get; set; }
    public required string DisplayName { get; set; }
    public required string Token { get; set; }
    
}