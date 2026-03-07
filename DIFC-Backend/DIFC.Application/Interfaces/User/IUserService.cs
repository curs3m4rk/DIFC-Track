using DIFC.Application.DTOs.Auth;

namespace DIFC.Application.Interfaces.User
{
    public interface IUserService
    {
        Task<AuthResultDTO> RegisterAsync(RegisterRequestDTO request);

    }
}
