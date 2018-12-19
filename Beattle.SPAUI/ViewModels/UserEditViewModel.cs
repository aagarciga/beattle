using Beattle.Infrastructure.Security;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Beattle.SPAUI.ViewModels
{
    public class UserEditViewModel : UserViewModel
    {
        public string CurrentPassword { get; set; }

        [MinLength(Security.PasswordRequiredLength)]
        public string NewPassword { get; set; }
        new private bool IsLockedOut { get; } //Hide base member
    }
}
