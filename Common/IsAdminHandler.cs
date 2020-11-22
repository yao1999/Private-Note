using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Private_Note.Common
{
    //public class IsAdminHandler : AuthorizationHandler<IsAdminRequirement>
    //{
    //    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, IsAdminRequirement requirement)
    //    {
    //        if (context.User.HasClaim(c => c.Type == ClaimTypes.))
    //        {
    //            //TODO: Use the following if targeting a version of
    //            //.NET Framework older than 4.6:
    //            //      return Task.FromResult(0);
    //            return Task.CompletedTask;
    //        }
    //    }
    //}
}
