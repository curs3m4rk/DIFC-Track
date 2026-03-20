using DIFC.Application.DTOs.Auth;
using DIFC.Application.Interfaces.Auth;
using DIFC.Application.Interfaces.Email;
using DIFC.Domain.Entities;
using DIFC.Domain.Entities.Auth;
using DIFC.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace DIFC.Application.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _config;
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<AuthService> _logger;

        public AuthService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ITokenService tokenService, ApplicationDbContext dbContext, ILogger<AuthService> logger, IEmailService emailService, IConfiguration config)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _dbContext = dbContext;
            _logger = logger;
            _emailService = emailService;
            _config = config;
        }


        
        #region LoginAsync

        public async Task<(bool Success, string? Error, LoginResponseDTO? Data)> LoginAsync(LoginRequestDTO request)
        {
            try
            {
                // Step 1: Get the user
                var user = await _userManager.FindByEmailAsync(request.Email);

                if(user is null)
                {
                    _logger.LogWarning("Login attempt failed: User with email {Email} not found", request.Email);
                    return (false, "Invalid Email or Password.", null);
                }

                // Step 2: Validate Password + Handle lockout  
                // CheckPasswordSignInAsync does THREE things in one call:
                // 1. Verifies the password hash
                // 2. Increments AccessFailedCount on failure
                // 3. Locks the account when MaxFailedAccessAttempts is reached
                // The lockoutOnFailure: true parameter enables that auto-lockout.
                var signInResult = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

                if(signInResult.IsLockedOut)
                {
                    _logger.LogWarning("Locked out account login attempt: {Email}", request.Email);
                    return (false, "Account is temporarily locked due to multiple failed attempts. Please try again later.", null);
                }

                if (!signInResult.Succeeded)
                {
                    _logger.LogWarning("Invalid password for: {Email}", request.Email);
                    return (false, "Invalid email or password.", null);
                }

                // Step 3: Get User Roles
                var roles = await _userManager.GetRolesAsync(user);

                // Step 4: Generate Tokens
                var accessToken = _tokenService.GenerateAccessToken(user, roles);
                var refreshToken = _tokenService.GenerateRefreshToken();
                refreshToken.UserId = user.Id;

                // Step 5: Cleanup old Refresh tokens
                // Revoke old tokens beyond the most recent 5 (one per device).
                // Prevents the DB from accumulating thousands of old tokens over time.
                // This is a simple "keep last N" cleanup strategy.
                var activeTokens = await _dbContext.RefreshTokens
                    .Where(rt => rt.UserId == user.Id && rt.RevokedAt == null && rt.ExpiresAt > DateTime.UtcNow)
                    .OrderByDescending(rt => rt.CreatedAt)
                    .Skip(4)
                    .ToListAsync();

                foreach(var old in activeTokens)
                    old.RevokedAt = DateTime.UtcNow;

                // Step 6: Save new Refresh token
                _dbContext.RefreshTokens.Add(refreshToken);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("User {Email} logged in successfully", request.Email);

                return (true, null, new LoginResponseDTO
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken.Token,
                    AccessTokenExpiry = DateTime.UtcNow.AddMinutes(15), 
                    UserId = user.Id,
                    Email = user.Email,
                    UserName = user.UserName,
                    Roles = roles
                });

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }
        #endregion

        #region RefreshAsync

        /// <summary>
        /// Validates the existing token pair and issues a fresh one.
        /// 
        /// Security checks in order:
        /// 1. Can we read the expired access token? (signature valid?)
        /// 2. Does the refresh token exist in our DB?
        /// 3. Does the refresh token belong to the same user as the access token?
        /// 4. Is the refresh token still active (not expired, not revoked)?
        /// 
        /// If ALL pass → rotate tokens → return new pair.
        /// If ANY fail → reject with 401.
        /// 
        public async Task<(bool Success, string? Error, LoginResponseDTO? Data)> RefreshAsync(RefreshTokenRequestDTO request)
        {
            // ── Step 1: Extract claims from the expired access token ────────
            // This validates the signature but ignores expiry.
            var principal = _tokenService.GetPrincipalFromExpiredToken(request.AccessToken);

            if (principal is null)
            {
                _logger.LogWarning("Refresh failed: invalid access token provided.");
                return (false, "Invalid access token.", null);
            }

            // Extract the userId that was baked into the JWT claims at login time
            // JwtRegisteredClaimNames.Sub = "sub" claim = userId
            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? principal.FindFirstValue(JwtRegisteredClaimNames.Sub);

            if (userId is null)
            {
                _logger.LogWarning("Refresh failed: no userId claim found in access token.");
                return (false, "Invalid access token.", null);

            }

            // ── Step 2: Find the refresh token in DB ────────────────────────
            // We load the User too (Include) because we need their data
            // to generate a new access token.
            var storedToken = await _dbContext.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken);

            if (storedToken is null)
            {
                _logger.LogWarning($"Refresh failed: refresh token not found. UserId: {userId}");
                return (false, "Invalid refresh token.", null);
            }

            // ── Step 3: Verify the refresh token belongs to the access token's user ─
            // Prevents one user from using another user's refresh token
            // combined with their own access token.
            if (storedToken.UserId != userId)
            {
                _logger.LogWarning($"Refresh failed: token/user mismatch. TokenUserId: {storedToken.UserId}, ClaimUserId: {userId}");
                return (false, "Invalid refresh token.", null);
            }

            // ── Step 4: Check if the refresh token is still usable ──────────
            if (storedToken.IsExpired)
            {
                _logger.LogWarning($"Refresh failed: token expired for UserId: {userId}");
                return (false, "Refresh token has expired. Please log in again.", null);
            }

            if (storedToken.IsRevoked)
            {
                _logger.LogWarning($"Revoked refresh token used! Possible theft. UserId: {userId}");
                return (false, "Refresh token has been revoked. Please log in again.", null);
            }

            // ── Step 5: TOKEN ROTATION ───────────────────────────────────────
            // Revoke the OLD refresh token immediately.
            // Generate a brand new refresh token.
            // Old one can never be used again.
            storedToken.RevokedAt = DateTime.UtcNow;

            var user = storedToken.User;
            var roles = await _userManager.GetRolesAsync(user);

            var newAccessToken = _tokenService.GenerateAccessToken(user, roles);
            var newRefreshToken = _tokenService.GenerateRefreshToken();
            newRefreshToken.UserId = user.Id;

            // ── Step 6: Save new refresh token ──────────────────────────────
            _dbContext.RefreshTokens.Add(newRefreshToken);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation($"Token refreshed successfully for {user.Email}");

            return (true, null, new LoginResponseDTO
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken.Token,
                AccessTokenExpiry = DateTime.UtcNow.AddMinutes(15),
                UserId = user.Id,
                Email = user.Email!,
                UserName = user.UserName,
                Roles = roles
            });
        }
        #endregion RefreshAsync

        #region LogoutAsync

        /// <summary>
        /// Revokes the refresh token, ending the session.
        /// 
        /// After this:
        /// - Refresh token is dead in DB → can never get new access tokens
        /// - Current access token lives until its 15 min natural expiry
        ///   but that's acceptable — it'll die on its own shortly
        ///   
        /// </summary>
        public async Task<(bool Success, string? Error)> LogoutAsync(LogoutRequestDTO request)
        {
            // ── Step 1: Find the token in DB ────────────────────────────────
            var storedToken = await _dbContext.RefreshTokens
                .Include(r => r.User)
                .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken);

            if (storedToken is null)
            {
                _logger.LogWarning("Logout failed: refresh token not found.");
                return (false, "Invalid refresh token.");
            }

            // ── Step 2: Check it's not already revoked ──────────────────────
            // Could mean duplicate logout call or a token that was already
            // cleaned up. Not a security issue, just inform the client.
            if(storedToken.IsRevoked)
            {
                _logger.LogWarning($"Logout called on already-revoked token. UserId: {storedToken.UserId}");
                return (false, "Token is already revoked.");
            }

            // ── Step 3: Revoke it ───────────────────────────────────────────
            storedToken.RevokedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation($"User {storedToken.User.Email} logged out successfully.");

            return (true, null);
        }
        #endregion LogoutAsync

        #region ForgotPasswordAsync
        /// <summary>
        /// Handles Step 1 of the reset flow.
        ///
        /// SECURITY DESIGN DECISION — Always return success:
        /// Whether the email exists or not, we return the same response.
        /// WHY? If we say "email not found" when it doesn't exist,
        /// attackers can enumerate valid emails in your system.
        /// Saying "if your email is registered you'll receive a link"
        /// reveals nothing — this is called a "oracle-resistant" response.
        /// </summary>
        public async Task<GenericResultDTO> ForgotPasswordAsync(string email)
        {
            // ── Step 1: Look up user 
            var user = await _userManager.FindByEmailAsync(email);

            // Return success even if user not found
            // We still log it internally for monitoring
            if (user is null)
            {
                _logger.LogWarning(
                    "Password reset requested for unknown email: {Email}", email);

                return new GenericResultDTO
                {
                    Success = true,
                    Message = "If your email is registered, you will receive a password reset link."
                };
            }

            // ── Step 2: Delete any existing unused tokens for this user 
            // Prevents token accumulation and ensures only ONE valid
            // reset token exists at a time per user.
            var existingTokens = await _dbContext.PasswordResetTokens
                .Where(t => t.UserId == user.Id && !t.IsUsed)
                .ToListAsync();

            _dbContext.PasswordResetTokens.RemoveRange(existingTokens);

            // ── Step 3: Generate a cryptographically secure token 
            // WHY not use a Guid? Guids are not cryptographically random —
            // some versions are partially predictable. RandomNumberGenerator
            // uses the OS CSPRNG (Cryptographically Secure Pseudo-Random
            // Number Generator) — unpredictable and safe for security tokens.
            var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64))
                .Replace('+', '-')
                .Replace('/', '_')
                .TrimEnd('=');

            // WHY hash the token before storing?
            // If an attacker gets read access to your DB (SQL injection etc.),
            // they'd see reset tokens and could use them to take over accounts.
            // We store SHA256(token) in DB but send the raw token in the email.
            // Even with DB access, they can't reverse the hash to get the token.
            var tokenHash = HashToken(rawToken);

            // ── Step 4: Save token to DB 
            var resetToken = new PasswordResetToken
            {
                TokenHash = tokenHash,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15), // 15 min window
                CreatedAt = DateTime.UtcNow,
                IsUsed = false
            };

            _dbContext.PasswordResetTokens.Add(resetToken);
            await _dbContext.SaveChangesAsync();

            // ── Step 5: Build reset link and send email ───────────────────
            // Link points to your FRONTEND, not the API.
            // Frontend reads the token from URL and shows the reset form.
            // Then frontend calls POST /api/auth/reset-password with it.
            var frontendUrl = _config["AppSettings:FrontendBaseUrl"];
            var resetLink = $"{frontendUrl}/reset-password?token={rawToken}";

            await _emailService.SendPasswordResetEmailAsync(
                user.Email!, user.FullName, resetLink);

            _logger.LogInformation(
                "Password reset email sent to {Email}", user.Email);

            return new GenericResultDTO
            {
                Success = true,
                Message = $"Password reset email sent to {user.Email}"
            };

        }

        #endregion ForgotPasswordAsync

        #region ResetPasswordAsync
        /// <summary>
        /// Handles Step 3 — validates the token and sets the new password.
        ///
        /// Flow:
        /// 1. Hash the incoming raw token
        /// 2. Find matching hash in DB
        /// 3. Validate it's not expired/used
        /// 4. Reset password via Identity (handles hashing)
        /// 5. Mark token as used
        /// 6. Revoke ALL refresh tokens → forces re-login everywhere
        /// </summary>
        public async Task<GenericResultDTO> ResetPasswordAsync(ResetPasswordRequestDTO request)
        {
            // ── Step 1: Hash the incoming token to look up in DB 
            // We never store raw tokens — only hashes. So to find the
            // matching record, we hash what the user sent and compare hashes.
            var tokenHash = HashToken(request.Token);

            // ── Step 2: Find token in DB 
            var resetToken = await _dbContext.PasswordResetTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.TokenHash == tokenHash);

            // ── Step 3: Validate token ────────────────────────────────────
            // We give a generic message for ALL failure cases.
            // Don't say "token expired" vs "token not found" — it leaks info.
            if (resetToken is null || !resetToken.IsValid)
            {
                _logger.LogWarning(
                    "Invalid or expired reset token used. Hash: {Hash}", tokenHash);

                return new GenericResultDTO
                {
                    Success = false,
                    Message = "This reset link is invalid or has expired. Please request a new one."
                };
            }

            var user = resetToken.User;

            // ── Step 4: Reset the password via Identity 
            // GeneratePasswordResetTokenAsync creates a short-lived Identity
            // token needed by ResetPasswordAsync internally.
            // WHY do we need an Identity token if we have our own?
            // Because ResetPasswordAsync is Identity's method — it requires
            // its own token format. We use our DB token for the "is this request
            // valid?" check, then use Identity's mechanism for the actual reset.
            var identityToken = await _userManager
                .GeneratePasswordResetTokenAsync(user);

            var result = await _userManager.ResetPasswordAsync(
                user, identityToken, request.NewPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ",
                    result.Errors.Select(e => e.Description));
                _logger.LogWarning(
                    "Password reset failed for {Email}: {Errors}", user.Email, errors);
                
                return new GenericResultDTO
                {
                    Success = false,
                    Message = "Password reset failed. ",
                    Errors = errors.Split(", ")
                };
            }

            // ── Step 5: Mark token as used 
            // Prevents the same link being used again.
            // Even if someone intercepts the email after use, link is dead.
            resetToken.IsUsed = true;

            // ── Step 6: Revoke ALL refresh tokens 
            // Password changed = security event.
            // Force logout from every device. User must log in fresh everywhere.
            // This is the enterprise standard — same as what banks do.
            var allRefreshTokens = await _dbContext.RefreshTokens
                .Where(r => r.UserId == user.Id && r.RevokedAt == null)
                .ToListAsync();

            foreach (var token in allRefreshTokens)
                token.RevokedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation(
                "Password reset successful for {Email}. {Count} sessions revoked.",
                user.Email, allRefreshTokens.Count);

            return new GenericResultDTO
            {
                Success = true,
                Message = "Your password has been reset successfully. Please log in with your new password."
            };
        }
        #endregion ResetPasswordAsync

        #region private methods
        private static string HashToken(string token)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(token.Trim());
            var hash = SHA256.HashData(bytes);
            return Convert.ToBase64String(hash)
                .Replace('+', '-')
                .Replace('/', '_')
                .TrimEnd('=');
        }
        #endregion private methods

    }
}
