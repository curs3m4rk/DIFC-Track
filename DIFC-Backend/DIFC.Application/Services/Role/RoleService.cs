using DIFC.Application.DTOs.Auth;
using DIFC.Application.DTOs.Role;
using DIFC.Application.Interfaces.Role;
using DIFC.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DIFC.Application.Services.Role
{
    public class RoleService : IRoleService
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<RoleService> _logger;

        public RoleService(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager, ILogger<RoleService> logger)
        {
            _roleManager = roleManager;

            _userManager = userManager;

            _logger = logger;
        }

        #region CreateRoleAsync
        public async Task<RoleResultDTO> CreateRoleAsync(RoleRequestDTO request)
        {
            try
            {
                var roleExists = await _roleManager.RoleExistsAsync(request.RoleName);

                if (roleExists)
                {
                    return RoleResultDTO.FailureResult(new[] { "Role already exists" });
                }

                var role = new IdentityRole
                {
                    Name = request.RoleName
                };

                var result = await _roleManager.CreateAsync(role);

                if (!result.Succeeded)
                {
                    return RoleResultDTO.FailureResult(result.Errors.Select(e => e.Description));
                }

                return RoleResultDTO.SuccessResult("Role created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }
        #endregion


        #region GetAllRolesAsync
        public async Task<List<string>> GetAllRolesAsync()
        {
            try
            {
                return _roleManager.Roles.Select(r => r.Name).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }
        #endregion


        #region GetRoleByNameAsync
        public async Task<RoleResultDTO> GetRoleByNameAsync(string roleName)
        {
            try
            {
                var role = await _roleManager.FindByNameAsync(roleName);

                if (role == null)
                {
                    return RoleResultDTO.FailureResult(new[] { "Role not found" });
                }

                return new RoleResultDTO
                {
                    Success = true,
                    Message = "Role retrieved successfully",
                    Data = new
                    {
                        role.Id,
                        role.Name
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


        #region DeleteRoleAsync
        public async Task<RoleResultDTO> DeleteRoleAsync(string roleName)
        {
            try
            {
                var role= await _roleManager.FindByNameAsync(roleName);

                if (role == null)
                {
                    return RoleResultDTO.FailureResult(new[] { "Role not found" });
                }

                var result = await _roleManager.DeleteAsync(role);

                if (!result.Succeeded)
                {
                    return RoleResultDTO.FailureResult(result.Errors.Select(e => e.Description));
                }

                return RoleResultDTO.SuccessResult("Role deleted successfully");


            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }
        #endregion

        #region UpdateRoleAsync
        public async Task<RoleResultDTO> UpdateRoleAsync(UpdateRoleDTO request)
        {
            try
            {
                var role = await _roleManager.FindByNameAsync(request.RoleName);

                if(role == null)
                {
                    return RoleResultDTO.FailureResult(new[] { "Role not found" });
                }

                role.Name = request.NewRoleName;

                var result = await _roleManager.UpdateAsync(role);

                if (!result.Succeeded)
                {
                    return RoleResultDTO.FailureResult(result.Errors.Select(e => e.Description));
                }

                return RoleResultDTO.SuccessResult("Role updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }
        #endregion

        #region AssignRolesAsync
        public async Task<RoleResultDTO> AssignRolesAsync(AssignRoleRequest request)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(request.UserName);

                if (user == null)
                    return RoleResultDTO.FailureResult($"User '{request.UserName}' not found.");

                var existingRoles = await _userManager.GetRolesAsync(user);
                var rolesToAssign = new List<string>();

                foreach (var role in request.Roles)
                {
                    if (!await _roleManager.RoleExistsAsync(role))
                        return RoleResultDTO.FailureResult($"Role '{role}' does not exist.");

                    //the API can take single or multiple Roles at once as List and assign it to a user
                    if (!existingRoles.Contains(role))
                        rolesToAssign.Add(role);
                }

                //Do not assign role again if already assigned.
                if (!rolesToAssign.Any())
                    return RoleResultDTO.FailureResult("Role is already assigned.");

                var result = await _userManager.AddToRolesAsync(user, rolesToAssign);

                if (!result.Succeeded)
                    return RoleResultDTO.FailureResult(result.Errors.Select(e => e.Description));

                return RoleResultDTO.SuccessResult(
                    "Roles assigned successfully.",
                    new
                    {
                        UserId = request.UserName,
                        RolesAssigned = rolesToAssign
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }
        #endregion

        #region UnassignRolesAsync
        public async Task<RoleResultDTO> UnassignRolesAsync(AssignRoleRequest request)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(request.UserName);
                //Error if User not present
                if (user == null)
                    return RoleResultDTO.FailureResult($"User '{request.UserName}' not found.");

                var existingRoles = await _userManager.GetRolesAsync(user);
                var rolesToRemove = new List<string>();

                foreach (var role in request.Roles)
                {
                    //Error if role is not present
                    if (!await _roleManager.RoleExistsAsync(role))
                        return RoleResultDTO.FailureResult($"Role '{role}' does not exist.");

                    //Error if input role is already not assigned to the user
                    if (!existingRoles.Contains(role))
                        return RoleResultDTO.FailureResult($"Role '{role}' is not assigned to user.");

                    rolesToRemove.Add(role);
                }

                var result = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);

                if (!result.Succeeded)
                    return RoleResultDTO.FailureResult(result.Errors.Select(e => e.Description));

                return RoleResultDTO.SuccessResult(
                    "Roles removed successfully.",
                    new
                    {
                        UserId = request.UserName,
                        RolesRemoved = rolesToRemove
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