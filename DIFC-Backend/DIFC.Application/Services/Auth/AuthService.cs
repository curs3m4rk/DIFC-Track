using DIFC.Application.DTOs.Auth;
using DIFC.Application.Interfaces.Auth;
using DIFC.Domain.Entities;
using DIFC.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace DIFC.Application.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<AuthService> _logger;

        public AuthService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ITokenService tokenService, ApplicationDbContext dbContext, ILogger<AuthService> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _dbContext = dbContext;
            _logger = logger;
        }


        #region RegisterAsync
        public async Task<AuthResultDTO> RegisterAsync(RegisterRequestDTO request)
        {
            try
            {
                if (request.Password != request.ConfirmPassword)
                {
                    return AuthResultDTO.FailureResult("Passwords do not match");
                }

                var user = new ApplicationUser
                {
                    UserName = request.UserName,
                    FullName = request.UserName,
                    Email = request.Email
                };

                var result = await _userManager.CreateAsync(user, request.Password);

                if (!result.Succeeded)
                {
                    return AuthResultDTO.FailureResult(result.Errors.Select(e => e.Description));
                }

                return AuthResultDTO.SuccessResult("User registered successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }
        #endregion RegisterAsync

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


    }
}
