using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HomeOwners.Filters
{
    public class RequireAuthenticationAttribute : TypeFilterAttribute
    {
        public RequireAuthenticationAttribute() : base(typeof(AuthenticationFilter))
        {
        }

        private class AuthenticationFilter : IAuthorizationFilter
        {
            public void OnAuthorization(AuthorizationFilterContext context)
            {
                if (!context.HttpContext.User.Identity.IsAuthenticated)
                {
                    // Redirect to login page
                    context.Result = new RedirectToPageResult("/Account/Login", new { area = "Identity" });
                }
            }
        }
    }
}
