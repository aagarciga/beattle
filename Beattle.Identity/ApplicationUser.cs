using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace Beattle.Identity
{
    public class ApplicationUser : IdentityUser
    {
        /// <summary>
        /// Navigation property for the roles user belongs to
        /// </summary>
        public virtual ICollection<IdentityUserRole<string>> Roles { get; set; }

        /// <summary>
        /// Navigation property for the claims this user possesses
        /// </summary>
        public virtual ICollection<IdentityUserClaim<string>> Claims { get; set; }
    }
}
