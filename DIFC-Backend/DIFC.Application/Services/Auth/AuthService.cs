using DIFC.Application.DTOs.Auth;
using DIFC.Application.Interfaces.Auth;
using DIFC.Domain.Entities;
using DIFC.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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


    }
}
