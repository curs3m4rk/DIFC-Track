namespace DIFC.Application.DTOs.Auth
{
    /// <summary>
    /// Step 3 — User submits the token from the email link
    /// along with their new password.
    ///
    /// The token comes from the URL query param (?token=xxx).
    /// The frontend reads it and includes it in this request body.
    /// </summary>
    public class ResetPasswordRequestDTO
    {
        public string Token { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
