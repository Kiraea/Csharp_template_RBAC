using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
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
    private ITokenService _tokenServiceImplementation;

    public TokenService(IConfiguration config)
    {
        _config = config;
        
        _key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_config["Jwt:SigningKey"]!));
    }
    public string GenerateAccessToken(AppUser user, IList<string> roles)
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
            Issuer = _config["Jwt:Issuer"],
            IssuedAt = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddMinutes(30),
            SigningCredentials = creds,
            Subject = claimsIdentity
        };
        var token = tokenHandler.CreateJwtSecurityToken(tokenDescriptor);
        // we use write token cause if we print token it might just print system.token stuff not the details
        
        return tokenHandler.WriteToken(token);


    }


    public void PutTokensInsideCookie(string accessToken, string refreshToken, HttpContext context)
    {
        // basically to send the cookie to the browser 
        context.Response.Cookies.Append("accessToken", accessToken,
            new CookieOptions()
            {
                HttpOnly = true, // cannot access by js
                Expires = DateTimeOffset.UtcNow.AddMinutes(5),
                IsEssential = true, // if this is required for the web app
                SameSite = SameSiteMode.None, // forgot
                Secure = false // if https or http
            }
            );
        context.Response.Cookies.Append("refreshToken",refreshToken, 
            new CookieOptions()
            {
                HttpOnly = true, // cannot access by js
                Expires = DateTimeOffset.UtcNow.AddMinutes(5),
                IsEssential = true, // if this is required for the web app
                SameSite = SameSiteMode.None, // forgot
                Secure = false // if https or http
            });
        
    }
    

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber); // doesnt return byte so it modifies the var insidde
            return Convert.ToBase64String(randomNumber); // does some kind of encoding to convert arr to string
        }
        
        
    }

    public ClaimsPrincipal GetPrincipalFromExpiredAccessToken(string token)
    {
        var tokenValidationParameters= new TokenValidationParameters()
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = _key,
            ValidateLifetime = false
        };
        var tokenHandler = new JwtSecurityTokenHandler();
        SecurityToken securityToken;

        var principal = tokenHandler.ValidateToken(token,tokenValidationParameters, out securityToken);
        
        
        // f securityToken is of type JwtSecurityToken, then:
        //Cast it into a variable called jwtSecurityToken
        //And let me use it in the next line
        // basically we need to check the sha256 as well cause there's a vulneraiblity where they attackers can set the algorithm type to none which doesnt check it
        if (securityToken is JwtSecurityToken jwtSecurityToken &&
            jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
        {
            return principal;
        }
        throw new SecurityTokenException("Expired Access Token is not valid");
    }
    


}