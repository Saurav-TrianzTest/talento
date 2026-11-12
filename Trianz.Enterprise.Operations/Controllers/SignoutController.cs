using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Trianz.Enterprise.Operations.Filters;

namespace Trianz.Enterprise.Operations.Controllers
{
    public class SignoutController : Controller
    {
        // GET: Signout
        public ActionResult Index()
        {
            return View();
        }
        
        public ActionResult Completed()
        {
            HttpCookie cookie = Request.Cookies["TSWA-Last-User"];

            if (User.Identity.IsAuthenticated == false || cookie == null || StringComparer.OrdinalIgnoreCase.Equals(User.Identity.Name, cookie.Value))
            {
                string name = string.Empty;

                if (Request.IsAuthenticated)
                {
                    name = User.Identity.Name;
                }

                // Cognito-based signout: clear session and cookies
                Session.Clear();
                Session.Abandon();
                Response.Cookies.Clear();
                Response.Redirect("~/Login/Index");;

                cookie = new HttpCookie("TSWA-Last-User", name);
                Response.Cookies.Set(cookie);

                Response.AppendHeader("Connection", "close");
                Response.StatusCode = 401; // Unauthorized;
                Response.Clear();
                //should probably do a redirect here to the unauthorized/failed login page
                //if you know how to do this, please tap it on the comments below
                //Response.Write("Unauthorized. Reload the page to try again... click <a href='http://www.google.com'>here</a> to go out from here");

                Response.Write("Unauthorized. Click <a href='/'>here</a> to re-login... <br/>click <a href='http://www.google.com'>here</a> to go out from here");
                Response.End();



                return RedirectToAction("Index", "TRHome");

                //return RedirectToAction("NotAuthorized", "Error");
            }

            cookie = new HttpCookie("TSWA-Last-User", string.Empty)
            {
                Expires = DateTime.UtcNow.AddYears(-5)
            };

            Response.Cookies.Set(cookie);

            return RedirectToAction("Index", "TRHome");
        }
        [SkipAuthorizationFilter]
        public ActionResult SessionExpire()        
        {
            return PartialView("SessionExpire");
        }

		/// <summary>
		/// Send an OpenID Connect sign-out request.
		/// </summary>
		public void SignOut()
		{
			HttpContext.GetOwinContext().Authentication.SignOut(
					OpenIdConnectAuthenticationDefaults.AuthenticationType,
					CookieAuthenticationDefaults.AuthenticationType);
		}
	}
}