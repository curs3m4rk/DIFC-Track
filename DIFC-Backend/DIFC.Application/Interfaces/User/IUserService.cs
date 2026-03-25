using DIFC.Application.DTOs.Auth;
using DIFC.Application.DTOs.Role;
using DIFC.Application.DTOs.User;

namespace DIFC.Application.Interfaces.User
{
    public interface IUserService
    {
        Task<GenericResultDTO> RegisterAsync(RegisterRequestDTO request);
        Task <List<UserDetailsDTO>> GetAllUsersAsync();
        Task<UserResultDTO> GetUserByUserNameAsync(string userId);

    }
}
