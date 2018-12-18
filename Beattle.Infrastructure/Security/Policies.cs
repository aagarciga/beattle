using System;
using System.Collections.Generic;
using System.Text;

namespace Beattle.Infrastructure.Security
{
    public class Policies
    {
        ///<summary>
        ///Policy to allow viewing all user records
        ///</summary>
        public const string ViewUsers = "View Users";

        /// <summary>
        /// Policy to allow adding, deleting and updating all user records
        /// </summary>
        public const string ManageUsers = "Manage Users";
        /// <summary>
        /// Policy to allow viewing details of all roles
        /// </summary>
        public const string ViewRoles = "View Roles";
        /// <summary>
        /// Policy to allow viewing details of all or specific roles (Requires roleName as parameter)
        /// </summary>
        public const string ViewRoleByRoleNamePolicy = "View Role by RoleName";
        /// <summary>
        /// Policy to allow adding, removing and updating all roles
        /// </summary>
        public const string ManageRoles = "Manage Roles";
        /// <summary>
        /// Policy to allow assigning roles the user has access to (Requires new and current roles as parameter)
        /// </summary>
        public const string AssignAllowedRolesPolicy = "Assign Allowed Roles";

    }
}
