
namespace DIFC.Application.DTOs.Auth
{
    public class LoginResponseDTO
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime AccessTokenExpiry { get; set; }

        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;

        public IList<String> Roles { get; set; } = new List<string>();
    }
}
