using DIFC.Application.DTOs.Auth;

namespace DIFC.Application.Interfaces.Auth
{
    public interface IAuthService
    {
        Task<(bool Success, string? Error, LoginResponseDTO? Data)> LoginAsync(LoginRequestDTO request);
        Task<(bool Success, string? Error, LoginResponseDTO? Data)> RefreshAsync(RefreshTokenRequestDTO request);
        Task<(bool Success, string? Error)> LogoutAsync(LogoutRequestDTO request);
    }
}
