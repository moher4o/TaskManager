using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.Linq;
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
                //Regex.Matches(glurl.ToLower(), "taskmanager").Count > 1
                if (httpContext.User.Claims.Where(cl => cl.Type == "2FA").Select(cl => cl.Value).FirstOrDefault() == "true" && !httpContext.Request.Cookies.Any(c => c.Key == "Test_cookie" && c.Value == "CfDJ8FQQXKoRyUdDvRNz9BGHr8JIy1flxoQVv2BUOnrzwQcRuoxF08Hr33t13jmyc"))
                {
                    var contentRoot = env.ContentRootPath;
                    var glurl = Microsoft.AspNetCore.Http.Extensions.UriHelper.GetDisplayUrl(httpContext.Request);
                    if (!glurl.ToLower().EndsWith("users/secondauthenticationlogin"))
                    {
                        var pathToRedirect = string.Empty;
                        if (glurl.ToLower().Contains("taskmanager"))      //ако адреса е например : https://taskmanager.e-gov.bg//taskmanager//
                        {
                            pathToRedirect = "/TaskManager/users/SecondAuthenticationLogin";
                            
                        }
                        else
                        {
                            pathToRedirect = "/users/SecondAuthenticationLogin";
                        }
                        //httpContext.Response.Redirect(pathToRedirect);
                        string fullUrl = Microsoft.AspNetCore.Http.Extensions.UriHelper.GetEncodedUrl(httpContext.Request);
                        string strPathAndQuery = Microsoft.AspNetCore.Http.Extensions.UriHelper.GetEncodedPathAndQuery(httpContext.Request);
                        string host = fullUrl.Replace(strPathAndQuery, "/");
                        //httpContext.Response.Headers[HeaderNames.Location] = httpContext.Request.Host + pathToRedirect;
                        var newPath = httpContext.Request.Host + pathToRedirect;
                        //httpContext.Response.Redirect(newPath);
                        httpContext = RedirectToCustomPath.RedirectToPath(httpContext, "Users", "SecondAuthenticationLogin");
                    }
                }
            }
            await _next(httpContext);
        }

    }
}
