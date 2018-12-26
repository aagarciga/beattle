using AspNet.Security.OpenIdConnect.Primitives;
using System.Linq;
using System.Security.Claims;

namespace Beattle.Infrastructure.Security
{
    public static class Security
    {
        

        public const bool PasswordRequiredDigit = true;
        public const int PasswordRequiredLength = 6;
        public const bool PasswordRequireNonAlphanumeric = true;
        public const bool PasswordRequireUppercase = true;
        public const bool PasswordRequireLowercase = true;
        public const int PasswordRequiredUniqueChars = 1;

        public static string PasswordRequiredLengthErrorMessage { get => string.Format("{0}", PasswordRequiredLength);}


        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static string GetUserId(ClaimsPrincipal user)
        {
            return user.FindFirst(OpenIdConnectConstants.Claims.Subject)?.Value?.Trim();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <param name="targetUserId"></param>
        /// <returns></returns>
        public static bool IsSameUser(ClaimsPrincipal user, string targetUserId)
        {
            if (string.IsNullOrWhiteSpace(targetUserId))
                return false;

            return GetUserId(user) == targetUserId;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="identity"></param>
        /// <returns></returns>
        public static string[] GetRoles(ClaimsPrincipal identity)
        {
            return identity.Claims
                .Where(c => c.Type == OpenIdConnectConstants.Claims.Role)
                .Select(c => c.Value)
                .ToArray();
        }
    }
}
