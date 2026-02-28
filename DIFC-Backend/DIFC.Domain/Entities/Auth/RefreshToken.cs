using System;
using System.Collections.Generic;
using System.Text;

namespace DIFC.Domain.Entities.Auth
{
    /// <summary>
    /// Represents a refresh token stored in the database.
    /// 
    /// WHY store refresh tokens in DB?
    /// Because JWT access tokens are STATELESS — once issued, 
    /// you can't invalidate them before they expire.
    /// Refresh tokens are STATEFUL — stored in DB, so you CAN 
    /// revoke them instantly (logout, security breach, etc.)
    /// 
    /// The flow:
    /// Access Token (15 min) ──expires──► Client uses Refresh Token
    ///                                    ──► Server checks DB ──► Issues new Access Token
    /// </summary>
    public class RefreshToken
    {
        public int Id { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? RevokedAt { get; set; }

        // Computed properties (not stored in db)
        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
        public bool IsRevoked => RevokedAt != null;
        public bool IsActive => !IsExpired && !IsRevoked;

        /// Foreign key to ApplicationUser.
        /// Each user can have multiple refresh tokens 
        /// (different devices / browsers).
        public string UserId { get; set; }  = string.Empty;
        public ApplicationUser User { get; set; } = null;

    }
}
