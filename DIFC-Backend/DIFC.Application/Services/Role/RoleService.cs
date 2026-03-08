using DIFC.Application.DTOs.Auth;
using DIFC.Application.DTOs.Role;
using DIFC.Application.Interfaces.Role;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace DIFC.Application.Services.Role
{
    public class RoleService : IRoleService
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<RoleService> _logger;

        public RoleService(RoleManager<IdentityRole> roleManager, ILogger<RoleService> logger)
        {
            _roleManager = roleManager;
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


        #region GetRoleByNameAsync
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
        
    }
}