using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;

namespace Trianz.Enterprise.Operations.Filters
{
    public class AuthorizeActionFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)

        {
            HttpContext ctx = HttpContext.Current;
            Controller controller = filterContext.Controller as Controller;
            
            if (controller != null)
            {
                if (filterContext.ActionDescriptor.GetCustomAttributes(typeof(SkipAuthorizationFilterAttribute), false).Any())
                {
                    return;
                }

                if (filterContext.HttpContext.Session.IsNewSession)
                {
                    string sessionCookie = ctx.Request.Headers["Cookie"];
                    //if ((null != sessionCookie) && (sessionCookie.IndexOf("ASP.NET_SessionId") >= 0))
                    if (HttpContext.Current?.Session["Role"] == null)
                    {
                        //string redirectOnSuccess = filterContext.HttpContext.Request.Url.PathAndQuery;
                        //string redirectUrl = string.Format("?ReturnUrl={0}", redirectOnSuccess);
                        //string loginUrl = FormsAuthentication.LoginUrl + redirectUrl;
                        //if (ctx.Request.IsAuthenticated)
                        //{
                        //    FormsAuthentication.SignOut();
                        //}
                        //RedirectResult rr = new RedirectResult("~/Signout/SessionExpire");
                        //filterContext.Result = rr;
                    }
                }
                //else
                //{
                //    // if (session == null && session["authstatus"] == null)
                //    //if (HttpContext.Current?.Session["Role"] == null)
                //    if (HttpContext.Current?.Session["Role"] == null)
                //    {
                //        //filterContext.Result =
                //        //       new RedirectToRouteResult(
                //        //           new RouteValueDictionary{{ "controller", "signout" },
                //        //                  { "action", "completed" }

                //        //                                         });
                //        filterContext.Result = new RedirectResult("~/Signout/Completed");
                //        //HttpResponse htp = new HttpResponse();
                //        //htp.Write("Unauthorized. Click <a href='/'>here</a> to re-login... <br/>click <a href='http://www.google.com'>here</a> to go out from here");
                //        return;
                //    }
                //}
                
                //HttpContext ctx = HttpContext.Current;
                //if (HttpContext.Current?.Session["ID"] == null)
                //{
                //    filterContext.Result = new RedirectResult("~/Signout/Completed");
                //    return;
                //}
                base.OnActionExecuting(filterContext);
            }
        }
    }
}