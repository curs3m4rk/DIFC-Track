using DIFC.Application.DTOs.User;
using DIFC.Application.DTOs.Auth;

namespace DIFC.Application.Interfaces.User
{
    public interface IUserService
    {
        Task<AuthResultDTO> RegisterAsync(RegisterRequestDTO request);

        //Task<List<UserDTO>> GetAllUsers(RegisterRequestDTO request);

    }
}
