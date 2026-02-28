using DIFC.Application.DTOs.Auth;

namespace DIFC.Application.Interfaces.Auth
{
    public interface IAuthService
    {
        Task<AuthResultDTO> RegisterAsync(RegisterRequestDTO request);
        Task<(bool Success, string? Error, LoginResponseDTO? Data)> LoginAsync(LoginRequestDTO request);
        Task<(bool Success, string? Error, LoginResponseDTO? Data)> RefreshAsync(RefreshTokenRequestDTO request);
    }
}
