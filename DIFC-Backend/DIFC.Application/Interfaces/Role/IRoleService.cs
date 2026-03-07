using DIFC.Application.DTOs.Auth;
using DIFC.Application.DTOs.Role;

namespace DIFC.Application.Interfaces.Role
{
    public interface IRoleService
    {
        Task<RoleResultDTO> CreateRoleAsync(RoleRequestDTO request);

        Task<List<string>> GetAllRolesAsync();

        Task<RoleResultDTO> GetRoleByNameAsync(string roleName);
    }
}