using WebApplication1.Models;

namespace WebApplication1.Interfaces;

public interface ITokenService
{
   string GenerateToken(AppUser user, IList<string> userRoles);
}