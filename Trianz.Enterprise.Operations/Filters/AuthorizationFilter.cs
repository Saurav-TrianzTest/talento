using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Session;

namespace Trianz.Enterprise.Operations.Filters
{
    public class AuthorizeActionFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var httpContext = filterContext.HttpContext;
            Controller controller = filterContext.Controller as Controller;

            if (controller != null)
            {
                if (filterContext.ActionDescriptor.EndpointMetadata.Any(em => em.GetType().Name == "SkipAuthorizationFilterAttribute"))
                {
                    return;
                }

                if (httpContext.Session != null)
                {
                    string role = httpContext.Session.GetString("Role");
                    if (role == null)
                    {
                        // Session expired or not authenticated
                    }
                }

                base.OnActionExecuting(filterContext);
            }
        }
    }
}
