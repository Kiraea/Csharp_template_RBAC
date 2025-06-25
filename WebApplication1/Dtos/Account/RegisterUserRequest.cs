using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Dtos.Account;

public record RegisterUserRequest
{
    [Required]
    [EmailAddress]
    public required string Email { get; set; }
    
     
    [Required]
    [MinLength(3, ErrorMessage = "Password must be at least 3 characters.")]
    [RegularExpression(@"^.{3,}$", ErrorMessage = "Password must be at least 3 characters long.")] 
    public required string Password { get; set; }
    
    
    
    [Required]
    public required string DisplayName { get; set; }
}