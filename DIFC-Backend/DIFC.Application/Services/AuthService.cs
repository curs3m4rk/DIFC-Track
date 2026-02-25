using DIFC.Application.DTOs.Auth;
using DIFC.Application.Interfaces;
using DIFC.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace DIFC.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AuthService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }
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
            catch (Exception)
            {
                throw;
            }
        }
    }
}
