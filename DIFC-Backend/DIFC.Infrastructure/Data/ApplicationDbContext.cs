using DIFC.Domain.Entities;
using DIFC.Domain.Entities.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DIFC.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        /// Our custom table for refresh tokens.
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // ALWAYS call base first — this sets up all Identity tables
            base.OnModelCreating(builder);

            // ── Orename Identity tables to cleaner names ──
            // By default they're named "AspNetUsers", "AspNetRoles" etc.
            // This renames them to just "Users", "Roles" etc. for a cleaner DB.
            builder.Entity<ApplicationUser>().ToTable("Users");
            builder.Entity<IdentityRole>().ToTable("Roles");
            builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
            builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
            builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
            builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");
            builder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");

            // ── RefreshToken → User relationship ──
            // One User can have MANY RefreshTokens (one per device/browser)
            // If User is deleted, all their RefreshTokens are deleted too (Cascade)
            builder.Entity<RefreshToken>()
                .HasOne(rt => rt.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<PasswordResetToken>()
                .HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<PasswordResetToken>()
                .HasIndex(p => p.TokenHash)
                .IsUnique();

        }
    }
}