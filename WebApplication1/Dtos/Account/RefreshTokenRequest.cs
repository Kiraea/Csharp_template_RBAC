using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Dtos.Account;

public record RefreshTokenRequest()
{
    [Required]
    public string AccessToken { get; set; }
    [Required]
    public string RefreshToken { get; set; }
};