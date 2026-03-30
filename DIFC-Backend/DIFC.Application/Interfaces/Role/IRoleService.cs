using DIFC.Application.DTOs.Auth;
using DIFC.Application.DTOs.Role;

namespace DIFC.Application.Interfaces.Role
{
    public interface IRoleService
    {
        Task<RoleResultDTO> CreateRoleAsync(RoleRequestDTO request);

        Task<List<string>> GetAllRolesAsync();

        Task<RoleResultDTO> GetRoleByNameAsync(string roleName);

        Task<RoleResultDTO> DeleteRoleAsync(string roleName);

        Task<RoleResultDTO> UpdateRoleAsync(UpdateRoleDTO request);

        Task<RoleResultDTO> AssignRolesAsync(AssignRoleRequest request);

        Task<RoleResultDTO> UnassignRolesAsync(AssignRoleRequest request);
    }
}