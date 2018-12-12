using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace Beattle.Identity
{
    public class ApplicationUser : IdentityUser
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ApplicationUser"/>
        /// </summary>
        /// <remarks>
        /// The Id property is initialized to from a new GUID string value
        /// </remarks>
        public ApplicationUser()
        {
            Roles = new HashSet<IdentityUserRole<string>>();
            Claims = new HashSet<IdentityUserClaim<string>>();
        }

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
