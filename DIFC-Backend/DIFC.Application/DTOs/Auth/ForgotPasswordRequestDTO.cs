namespace DIFC.Application.DTOs.Auth
{
    /// <summary>
    /// Step 1 — User provides their email.
    /// We look them up, generate a token, send the email.
    ///
    /// WHY only email and not username?
    /// Email is the unique identifier in your system (RequireUniqueEmail = true).
    /// It's also where we send the reset link, so we need it anyway.
    /// </summary>
    public class ForgotPasswordRequestDTO
    {
        public string Email { get; set; } = string.Empty;
    }
}
