using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Trianz.Enterprise.Operations.Filters
{
    public class HandleErrorAttribute : ActionFilterAttribute, IExceptionFilter
    {
        public Type ExceptionType { get; set; }
        public string View { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }

        public HandleErrorAttribute()
        {
        }

        public void OnException(ExceptionContext filterContext)
        {
            // If the exception is not of the configured type, ignore it
            if (ExceptionType != null && !filterContext.Exception.GetType().IsAssignableFrom(ExceptionType))
            {
                return;
            }

            // Mark the exception as handled
            filterContext.ExceptionHandled = true;
            filterContext.HttpContext.Response.StatusCode = 500;

            // Create HandleErrorInfo and pass to the error view
            var controllerName = filterContext.RouteData.Values["controller"]?.ToString() ?? string.Empty;
            var actionName = filterContext.RouteData.Values["action"]?.ToString() ?? string.Empty;

            var model = new Microsoft.AspNetCore.Mvc.HandleErrorInfo(actionName, controllerName, filterContext.Exception);

            var result = new ViewResult
            {
                ViewName = View ?? "Error",
                ViewData = new ViewDataDictionary<Microsoft.AspNetCore.Mvc.HandleErrorInfo>(new EmptyModelMetadataProvider(), new ModelStateDictionary())
                {
                    Model = model
                },
                StatusCode = 500
            };

            if (!string.IsNullOrEmpty(Controller))
            {
                result = new ViewResult
                {
                    ViewName = Action ?? "Error",
                    ViewData = new ViewDataDictionary<Microsoft.AspNetCore.Mvc.HandleErrorInfo>(new EmptyModelMetadataProvider(), new ModelStateDictionary())
                    {
                        Model = model
                    },
                    StatusCode = 500
                };
            }

            filterContext.Result = result;
        }
    }
}
