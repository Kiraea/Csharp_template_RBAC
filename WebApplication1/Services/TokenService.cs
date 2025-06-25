using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using WebApplication1.Interfaces;
using WebApplication1.Models;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace WebApplication1.Services;

public class TokenService : ITokenService
{
    
    // to access the .json config
    private readonly IConfiguration _config;
    
    // signingkey in json
    private readonly SymmetricSecurityKey _key;
    public TokenService(IConfiguration config)
    {
        _config = config;
        
        _key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_config["Jwt:SigningKey"]!));
    }
    public string GenerateToken(AppUser user, IList<string> roles)
    {
        // to put in signingcredentials of token descriptor
        var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim> 
        {
            // specifically for microsoft stuff
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            // the reason why sub is that its default of jwt identrifaction
            // if other systems be using that are non .net this is the way
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            // Generic Claim name, used for display name, doesn't have to be unique
            new Claim(ClaimTypes.Name, user.DisplayName.ToString()),
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var claimsIdentity = new ClaimsIdentity(claims);
        
        
        var tokenHandler = new JwtSecurityTokenHandler(); // this needs a dedscriptor check docs
        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            Audience = _config["Jwt:Audience"],
            Issuer = _config["Jwt:"],
            IssuedAt = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddMinutes(30),
            SigningCredentials = creds,
            Subject = claimsIdentity
        };
        var token = tokenHandler.CreateJwtSecurityToken(tokenDescriptor);
        // we use write token cause if we print token it might just print system.token stuff not the details
        
        return tokenHandler.WriteToken(token);


    }
}