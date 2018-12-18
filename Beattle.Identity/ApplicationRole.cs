using Beattle.Application.Interfaces;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Beattle.Identity
{
    public class ApplicationRole : IdentityRole, IApplicationRole
    {
        /// <summary>
        /// Get or sets the description for this role
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Initializes a new instance of <see cref="ApplicationRole"/>
        /// </summary>
        /// <remarks>
        /// The Id property is initialized to from a new GUID string value
        /// </remarks>
        public ApplicationRole()
        {
            Users = new HashSet<IdentityUserRole<string>>();
            Claims = new HashSet<IdentityRoleClaim<string>>();
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ApplicationRole"/>
        /// </summary>
        /// <param name="name">The role name</param>
        /// <remarks>
        /// The Id property is initialized to from a new GUID string value
        /// </remarks>
        public ApplicationRole(string name) : base(name)
        {
            Users = new HashSet<IdentityUserRole<string>>();
            Claims = new HashSet<IdentityRoleClaim<string>>();
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ApplicationRole"/>
        /// </summary>
        /// <param name="name">The role name</param>
        /// <param name="description">The role description</param>
        /// <remarks>
        /// The Id property is initialized to from a new GUID string value
        /// </remarks>
        public ApplicationRole(string name, string description) : base(name)
        {
            Description = description;
            Users = new HashSet<IdentityUserRole<string>>();
            Claims = new HashSet<IdentityRoleClaim<string>>();
        }

        /// <summary>
        /// Navigation property for the users in this role
        /// </summary>
        public virtual ICollection<IdentityUserRole<string>> Users { get; set; }

        /// <summary>
        /// Navigation property for the claims in this role
        /// </summary>
        public virtual ICollection<IdentityRoleClaim<string>> Claims { get; set; }
    }
}
