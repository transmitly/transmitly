using Microsoft.AspNetCore.Identity;

namespace VerifyV2Quickstart.Models
{
    public class ApplicationUser:IdentityUser
    {

        public bool Verified { get; set; }

    }
}