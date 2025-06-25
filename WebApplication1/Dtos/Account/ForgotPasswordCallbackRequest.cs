using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Dtos.Account;

public class ForgotPasswordCallbackRequest
{
    [Required]
    [EmailAddress]
    public required string Email { get; set; }
    
    
    public required string Token { get; set; }
    
    
    [Required]
    [MinLength(3, ErrorMessage = "Password must be at least 3 characters.")]
    [RegularExpression(@"^.{3,}$", ErrorMessage = "Password must be at least 3 characters long.")] 
    public required string Password { get; set; }
}