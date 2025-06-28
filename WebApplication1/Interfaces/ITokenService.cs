using System.Security.Claims;
using WebApplication1.Models;

namespace WebApplication1.Interfaces;

public interface ITokenService
{
   string GenerateAccessToken(AppUser user, IList<string> userRoles);

   public string GenerateRefreshToken();
   public ClaimsPrincipal GetPrincipalFromExpiredAccessToken(string token);
   public void PutTokensInsideCookie(string accessToken, string refreshToken, HttpContext context);
   
}