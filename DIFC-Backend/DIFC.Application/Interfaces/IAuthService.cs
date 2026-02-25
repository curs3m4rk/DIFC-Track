using DIFC.Application.DTOs.Auth;

namespace DIFC.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResultDTO> RegisterAsync(RegisterRequestDTO request);
    }
}
