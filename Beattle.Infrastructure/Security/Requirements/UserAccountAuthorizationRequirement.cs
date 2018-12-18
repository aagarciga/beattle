using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Beattle.Infrastructure.Security.Requirements
{
    public class UserAccountAuthorizationRequirement : IAuthorizationRequirement
    {
        public string OperationName { get; private set; }

        public UserAccountAuthorizationRequirement(string operationName)
        {
            OperationName = operationName;
        }
    }
}


