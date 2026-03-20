using DIFC.Application.DTOs.Auth;
using DIFC.Application.Interfaces.Auth;
using DIFC.Application.Interfaces.User;
using DIFC.Application.DTOs.User;
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
        private readonly ILogger<UserService> _logger;

        public UserService(UserManager<ApplicationUser> userManager, ILogger<UserService> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }


        #region RegisterAsync
        public async Task<GenericResultDTO> RegisterAsync(RegisterRequestDTO request)
        {
            try
            {
                var user = new ApplicationUser
                {
                    UserName = request.UserName,
                    FullName = request.UserName,
                    Email = request.Email
                };

                var result = await _userManager.CreateAsync(user, request.Password);

                if (!result.Succeeded)
                {
                    return GenericResultDTO.FailureResult(result.Errors.Select(e => e.Description));
                }

                return GenericResultDTO.SuccessResult("User registered successfully");
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
