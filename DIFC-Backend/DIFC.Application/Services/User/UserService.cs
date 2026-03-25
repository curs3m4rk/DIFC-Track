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
using DIFC.Application.DTOs.Role;

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

        #region GetAllUsersAsync
        public async Task<List<UserDetailsDTO>> GetAllUsersAsync()
        {
            try
            {
                var users = _userManager.Users.ToList();

                var result = new List<UserDetailsDTO>();

                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);

                    result.Add(new UserDetailsDTO
                    {
                        Id = user.Id,
                        UserName = user.UserName,
                        FullName = user.FullName,
                        Email = user.Email,
                        PhoneNumber = user.PhoneNumber,
                        Roles = roles.ToList()
                    });
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }
        #endregion

        #region GetRoleByNameAsync
        public async Task<UserResultDTO> GetUserByUserNameAsync(string userName)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(userName);

                if (user == null)
                {
                    return UserResultDTO.FailureResult(new[] { "User not found" });
                }

                var roles = await _userManager.GetRolesAsync(user);

                return new UserResultDTO
                {
                    Success = true,
                    Message = "User details retrieved successfully",
                    Data = new UserDetailsDTO
                    {
                        Id = user.Id,
                        UserName = user.UserName,
                        FullName = user.FullName,
                        Email = user.Email,
                        PhoneNumber = user.PhoneNumber,
                        Roles = roles.ToList()
                    }
            };
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
