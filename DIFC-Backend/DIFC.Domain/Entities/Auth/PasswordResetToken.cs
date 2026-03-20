/// <summary>
/// Stores password reset tokens in the database.
///
/// WHY store reset tokens in DB instead of using JWT?
/// Same reason as refresh tokens — we need to be able to:
/// 1. Expire them on a fixed schedule (15 min)
/// 2. Mark them as USED so they can't be reused
/// 3. Revoke them if suspicious activity is detected
///
/// A JWT can't be "used up" — it's valid until expiry no matter what.
/// A DB token can be deleted/flagged the moment it's consumed.
///
/// WHY NOT use ASP.NET Identity's built-in token provider?
/// Identity has built-in token generation (GeneratePasswordResetTokenAsync)
/// but those tokens are not stored in DB — they're HMAC-based stateless tokens.
/// That means you CAN'T expire them early or mark them as used.
/// For enterprise/govt apps, storing them gives you full control.
/// </summary>
namespace DIFC.Domain.Entities.Auth
{
    public class PasswordResetToken
    {
        public int Id { get; set; }

        /// <summary>
        /// The actual token — a cryptographically random string.
        /// This is what gets embedded in the reset link URL.
        /// We store a HASH of this (not the raw value) in the DB
        /// so even if DB is breached, tokens can't be used.
        /// </summary>
        public string TokenHash { get; set; } = string.Empty;

        /// <summary>
        /// Token dies after 15 minutes. Non-negotiable for security.
        /// Short window limits damage if the email is intercepted.
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Once used to reset a password, token is marked used.
        /// Prevents using the same reset link twice.
        /// </summary>
        public bool IsUsed { get; set; } = false;

        // ── Computed ──────────────────────────────────────────
        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
        public bool IsValid => !IsUsed && !IsExpired;

        // ── Relationship ──────────────────────────────────────
        /// <summary>
        /// Which user this token belongs to.
        /// One user can have multiple tokens (requested multiple times)
        /// but only the latest one is valid — old ones get deleted on
        /// each new request.
        /// </summary>
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;
    }
}
