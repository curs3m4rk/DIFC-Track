namespace DIFC.Application.DTOs.Auth
{
    /// <summary>
    /// Client sends both tokens when requesting a refresh.
    /// 
    /// WHY the expired access token?
    /// It contains the user's claims (id, roles) baked in.
    /// We use it to know WHO is refreshing without hitting the DB.
    /// We only skip the expiry check — signature is still verified.
    /// </summary>
    public class RefreshTokenRequestDTO
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}
