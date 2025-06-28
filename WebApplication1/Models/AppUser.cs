using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace WebApplication1.Models;

public class AppUser : IdentityUser
{
    
    
    
    // required here is compile time or static analysis of the IDE
    // runtime only checks if it actually goes to this class
    [Required] public required string DisplayName { get; set; } = "DefaultUser";
    
    public string? RefreshToken { get; set; }
    public DateTime RefreshTokenExpiry { get; set; }
    
    
}