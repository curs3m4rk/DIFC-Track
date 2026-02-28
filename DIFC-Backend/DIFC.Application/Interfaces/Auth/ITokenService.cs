using DIFC.Domain.Entities;
using DIFC.Domain.Entities.Auth;
using System.Security.Claims;

namespace DIFC.Application.Interfaces.Auth
{
    public interface ITokenService
    {
        string GenerateAccessToken(ApplicationUser user, IList<string> roles);
        RefreshToken GenerateRefreshToken();
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    }
}
