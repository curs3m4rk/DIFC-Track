using DIFC.Application.Interfaces.Auth;
using DIFC.Domain.Entities;
using DIFC.Domain.Entities.Auth;
using Microsoft.Extensions.Configuration;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;

namespace DIFC.Application.Services.Auth
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;

        public TokenService(IConfiguration config)
        {
            _config = config;
        }

        #region GenerateAccessToken
        /// <summary>
        /// Generates a JWT access token.
        /// 
        /// A JWT has 3 parts separated by dots: header.payload.signature
        /// - Header: algorithm used (HS256)
        /// - Payload: "claims" — data baked into the token (userId, email, roles)
        /// - Signature: HMAC hash of header+payload using your SecretKey
        ///   → Server can verify the token hasn't been tampered with
        /// 
        /// Claims are READABLE by anyone (base64 encoded, not encrypted).
        /// They are just TAMPER-PROOF. Never put sensitive data in claims.
        /// </summary>
        public string GenerateAccessToken(ApplicationUser user, IList<string> roles)
        {
            try
            {
                var jwtSettings = _config.GetSection("JwtSettings");
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!));

                var claims = new List<Claim>
                {
                    new(JwtRegisteredClaimNames.Sub, user.Id), // Subject (user ID)
                    new(JwtRegisteredClaimNames.Email, user.Email!), // User's email
                    new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // Unique token ID
                    new("fullName", user.FullName),
                };

                // Add each role as a separate claim
                // ASP.NET reads ClaimTypes.Role automatically for [Authorize(Roles="Admin")]
                claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

                var expiry = DateTime.UtcNow.AddMinutes(
                    double.Parse(jwtSettings["AccessTokenExpiryMinutes"]!));

                // generated token with all things
                var token = new JwtSecurityToken(
                    issuer: jwtSettings["Issuer"],
                    audience: jwtSettings["Audience"],
                    claims: claims,
                    expires: expiry,
                    signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
                );

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region GenerateRefreshToken
        /// <summary>
        /// Generates a cryptographically secure random refresh token.
        /// 
        /// WHY not use a JWT for the refresh token?
        /// Because refresh tokens need to be REVOCABLE. JWTs are stateless
        /// and can't be revoked. A random opaque string stored in DB can be
        /// deleted/revoked instantly.
        /// </summary>
        public RefreshToken GenerateRefreshToken()
        {
            try
            {
                var jwtSetings = _config.GetSection("JwtSettings");

                return new RefreshToken
                {
                    // RandomNumberGenerator is cryptographically secure (unlike Random.Next)
                    Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                    ExpiresAt = DateTime.UtcNow.AddDays(
                        double.Parse(jwtSetings["RefreshTokenExpiryDays"]!)),
                    CreatedAt = DateTime.UtcNow
                };
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion
    }
}
