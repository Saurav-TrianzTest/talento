using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Trianz.Enterprise.Operations.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Session;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace Trianz.Enterprise.Operations.Controllers
{
    public class SignoutController : Controller
    {
        // GET: Signout
        public ActionResult Index()
        {
            return View();
        }
        
        public async Task<ActionResult> Completed()
        {
            string cookieValue = Request.Cookies["TSWA-Last-User"];
            string cookieName = "TSWA-Last-User";

            if (User.Identity.IsAuthenticated == false || cookieValue == null || StringComparer.OrdinalIgnoreCase.Equals(User.Identity.Name, cookieValue))
            {
                string name = string.Empty;

                if (User.Identity.IsAuthenticated)
                {
                    name = User.Identity.Name;
                }

                await HttpContext.SignOutAsync();

                Response.Cookies.Append(cookieName, name, new CookieOptions());

                Response.Headers.Add("Connection", "close");
                Response.StatusCode = 401; // Unauthorized;
                
                await Response.WriteAsync("Unauthorized. Click <a href='/'>here</a> to re-login... <br/>click <a href='http://www.google.com'>here</a> to go out from here");

                return RedirectToAction("Index", "TRHome");
            }

            Response.Cookies.Append(cookieName, string.Empty, new CookieOptions
            {
                Expires = DateTime.Now.AddYears(-5)
            });

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
			// ASP.NET Core uses SignInManager or HttpContext.SignOutAsync
			// This method is kept for compatibility but may need proper implementation
			// based on the authentication scheme configured in the application
		}
	}
}