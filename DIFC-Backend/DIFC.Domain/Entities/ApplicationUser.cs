using DIFC.Domain.Entities.Auth;
using Microsoft.AspNetCore.Identity;

namespace DIFC.Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// Navigation property — EF Core uses this to load related refresh tokens.
        /// This is NOT a database column. It's just a C# convenience property
        /// that EF Core knows how to populate via a JOIN.
        /// </summary>
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}
