using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Others;

public class EmailConfiguration
{
    [Required]
    public required string From { get; set; }
    [Required]
    public required string SmtpServer { get; set; }
    [Required]
    public required int Port { get; set; }
    [Required]
    public required string Username { get; set; }
    [Required]
    public required string Password { get; set; }
    
    
}