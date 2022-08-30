using Microsoft.AspNetCore.Authorization;
using Cust.Areas.Identity.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace Cust.Authorization
{
    public class UserAdminHandler : AuthorizationHandler<AllowedManagementRequirement>
    {
        //protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AllowedManagementRequirement requirement)

        //UserManager<CustUser> _userManager;
        //public UserAdminHandler(UserManager<CustUser> userManager)
        //{
        //    _userManager = userManager;
        //}


        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AllowedManagementRequirement requirement)
        {
            //var usrs = _userManager.Users.ToArray();
            //var u0 = usrs[0];
            //u0.Alias = "newAlias";

            //if (context.User.Identity.Name == "1@1.1")

            if (context.User.Claims.Where(c => c.Type == "IsAdmin").FirstOrDefault() != null )
            {
                context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }
    }
}
