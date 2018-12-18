using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Beattle.Application.Interfaces
{
    public interface IApplicationDbContext
    {
        string CurrentUserId { get; set; }
        //IQueryable<IApplicationRole> Roles { get; set; }
        //IQueryable<IApplicationUser> Users { get; set; }
        //IQueryable<IdentityUserRole<string>> UserRoles { get; set; }
    }
}
