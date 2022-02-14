using Microsoft.AspNetCore.Hosting;
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

        public async Task InvokeAsync(HttpContext httpContext, IWebHostEnvironment env)
        {
            if (httpContext.User != null && httpContext.User.Identity.IsAuthenticated)
            {
                //var contentRoot = env.ContentRootPath;
                // httpContext.Request.Path.Value != "/users/SecondAuthenticationLogin"
                if (httpContext.User.Claims.Where(cl => cl.Type == "2FA").Select(cl => cl.Value).FirstOrDefault() == "true" && !httpContext.Request.Cookies.Any(c => c.Key == "Test_cookie" && c.Value == "CfDJ8FQQXKoRyUdDvRNz9BGHr8JIy1flxoQVv2BUOnrzwQcRuoxF08Hr33t13jmyc"))
                {
                    var contentRoot = env.ContentRootPath;
                    var glurl = Microsoft.AspNetCore.Http.Extensions.UriHelper.GetDisplayUrl(httpContext.Request);
                    if (!glurl.ToLower().EndsWith("users/secondauthenticationlogin"))
                    {
                        var pathToRedirect = string.Empty;
                        if (glurl.ToLower().Contains("localhost"))
                        {
                            pathToRedirect = "/users/SecondAuthenticationLogin";
                        }
                        else
                        {
                            pathToRedirect = "TaskManager/users/SecondAuthenticationLogin";
                        }
                        httpContext.Response.Redirect(pathToRedirect);
                    }
                }
            }
            await _next(httpContext);
        }

    }
}
