using DIFC.Application.DTOs.Auth;
using DIFC.Application.Interfaces.Auth;
using DIFC.Application.Interfaces.User;
using DIFC.Application.Services.Auth;
using DIFC.Domain.Entities;
using DIFC.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace DIFC.Application.Services.User
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AuthService> _logger;

        public UserService(UserManager<ApplicationUser> userManager, ILogger<AuthService> logger)
        {
            _userManager = userManager;
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

    }
}
