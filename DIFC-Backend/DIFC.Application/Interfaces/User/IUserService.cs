using DIFC.Application.DTOs.User;
using DIFC.Application.DTOs.Auth;

namespace DIFC.Application.Interfaces.User
{
    public interface IUserService
    {
        Task<GenericResultDTO> RegisterAsync(RegisterRequestDTO request);
        Task<List<string>> GetAllUsersAsync();
        Task<UserResultDTO> GetUserByIdAsync(string userId);

    }
}
