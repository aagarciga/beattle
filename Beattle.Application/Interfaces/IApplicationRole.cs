using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Beattle.Application.Interfaces
{
    public interface IApplicationRole
    {
        string Id { get; set; }
        string Name { get; set; }
        string Description { get; set; }
        ICollection<IdentityRoleClaim<string>> Claims { get; set; }
        ICollection<IdentityUserRole<string>> Users { get; set; }
    }
}
