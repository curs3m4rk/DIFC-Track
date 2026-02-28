namespace DIFC.Application.DTOs.Auth
{
    /// <summary>
    /// Client only needs to send the refresh token.
    /// We look it up in DB, verify it belongs to them, then revoke it.
    /// 
    /// WHY not require the access token too?
    /// Logout should work even if the access token just expired.
    /// The refresh token is enough to identify the session.
    /// </summary>
    public class LogoutRequestDTO
    {
        public string RefreshToken { get; set; } = string.Empty;
    }
}
