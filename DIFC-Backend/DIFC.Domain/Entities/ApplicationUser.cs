using Microsoft.AspNetCore.Identity;

namespace DIFC.Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
    }
}
