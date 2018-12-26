using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Beattle.Application.Interfaces
{
    public interface IApplicationUser
    {
        string Id { get; set; }
        string Name { get; set; }
        string UserName { get; set; }
        string Settings { get; set; }
        bool IsEnabled { get; set; }
        ICollection<IdentityUserClaim<string>> Claims { get; set; }
        ICollection<IdentityUserRole<string>> Roles { get; set; }
    }
}
