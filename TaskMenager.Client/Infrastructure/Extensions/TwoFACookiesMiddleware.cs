using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace TaskMenager.Client.Infrastructure.Extensions
{
    public class TwoFACookiesMiddleware
    {
        private readonly RequestDelegate _next;

        public TwoFACookiesMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            if (httpContext.User != null && httpContext.User.Identity.IsAuthenticated)
            {
                if (httpContext.User.Claims.Where(cl => cl.Type == "2FA").Select(cl => cl.Value).FirstOrDefault() == "true")
                {
                    if (!httpContext.Request.Cookies.Any(c => c.Key == "Test_cookie" && c.Value == "yo") && httpContext.Request.Path.Value != "/users/SecondAuthentication")
                    {
                        httpContext.Response.Redirect("/users/SecondAuthentication");
                    }
                }
            }
            await _next(httpContext);
        }

    }
}
