using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskMenager.Client.Infrastructure.Extensions
{
    public static class RedirectToCustomPath
    {

        public static HttpContext RedirectToPath(this HttpContext context, string controllerName, string actionName)
        {
            // Get the old endpoint to extract the RequestDelegate
            var currentEndpoint = context.GetEndpoint();

            // Get access to the action descriptor collection
            var actionDescriptorsProvider =
                context.RequestServices.GetRequiredService<IActionDescriptorCollectionProvider>();

            // Get the controller aqction with the action name and the controller name.
            // You should be redirecting to a GET action method anyways. Anyone can provide a better way of achieving this. 
            var controllerActionDescriptor = actionDescriptorsProvider.ActionDescriptors.Items
                .Where(s => s is ControllerActionDescriptor bb
                            && bb.ActionName == actionName
                            && bb.ControllerName == controllerName
                            && (bb.ActionConstraints == null
                                || (bb.ActionConstraints != null
                                   && bb.ActionConstraints.Any(x => x is HttpMethodActionConstraint cc
                                   && cc.HttpMethods.Contains(HttpMethods.Get))))
                                   )
                .Select(s => s as ControllerActionDescriptor)
                .FirstOrDefault();

            if (controllerActionDescriptor is null) throw new Exception($"You were supposed to be redirected to {actionName} but the action descriptor could not be found.");

            // Create a new route endpoint
            // The route pattern is not needed but MUST be present. 
            var routeEndpoint = new RouteEndpoint(currentEndpoint.RequestDelegate, RoutePatternFactory.Parse(""), 1, new EndpointMetadataCollection(new object[] { controllerActionDescriptor }), controllerActionDescriptor.DisplayName);

            // set the new endpoint. You are assured that the previous endpoint will never execute.
            context.SetEndpoint(routeEndpoint);
            return context;
        }
    }
}
