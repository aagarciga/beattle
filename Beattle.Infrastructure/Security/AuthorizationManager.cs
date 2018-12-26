using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Beattle.Infrastructure.Security
{
    public static class AuthorizationManager
    {
        public static IReadOnlyCollection<Authorization> Authorizations;

        #region Application Authorizations Declaration
        public const string AUTHORIZATION_GROUP_ROLES = "Roles Authorizations";
        public static Authorization ViewRoles = new Authorization(
            "View Roles",
            "roles.view",
            AUTHORIZATION_GROUP_ROLES,
            "Authorization to view available roles"
            );
        public static Authorization ManageRoles = new Authorization(
            "Manage Roles",
            "roles.manage",
            AUTHORIZATION_GROUP_ROLES,
            "Authorization to create, update and delete roles"
            );
        public static Authorization AssignRoles = new Authorization(
            "Assign Roles",
            "roles.assign",
            AUTHORIZATION_GROUP_ROLES,
            "Authorization to assign roles to users"
            );

        public const string AUTHORIZATION_GROUP_USERS = "User Authorizations";
        public static Authorization ViewUsers = new Authorization(
            "View Users",
            "users.view",
            AUTHORIZATION_GROUP_USERS,
            "Authorization to view other users account details"
            );
        public static Authorization ManageUsers = new Authorization(
            "Manage Users",
            "user.manage",
            AUTHORIZATION_GROUP_USERS,
            "Authorization to create, update and delete other users account details"
            );
        #endregion

        static AuthorizationManager()
        {
            List<Authorization> authorizations = new List<Authorization>()
            {
                ViewRoles,
                ManageRoles,
                AssignRoles,

                ViewUsers,
                ManageUsers
            };
            Authorizations = authorizations.AsReadOnly();
        }

        public static Authorization GetByName(string name)
        {
            return Authorizations.Where(authorization => authorization.Name == name).FirstOrDefault();
        }

        public static Authorization GetByValue(string value)
        {
            return Authorizations.Where(authorization => authorization.Value == value).FirstOrDefault();
        }

        public static string[] GetAllAuthorizationValues()
        {
            return Authorizations.Select(authorization => authorization.Value).ToArray();
        }

        public static string[] GetAdministrativeAuthorizationValues()
        {
            return new string[] { ManageUsers, ManageRoles, AssignRoles };
        }
    }
}
