using System;
using System.Collections.Generic;
using System.Text;

namespace Beattle.Identity
{
    public static class ApplicationClaimType
    {
        /// <summary>
        /// A claim that specifies the permission of an entity
        /// </summary>
        public const string Authorization = "authorization";

        /// <summary>
        /// A claim that specifies the setting of an entity
        /// </summary>
        public const string Setting = "setting";

        public const string Name = "fullname";
        public const string Email = "email";
        public const string PhoneNumber = "phonenumber";
    }
}
