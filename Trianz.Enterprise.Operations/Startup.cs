using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Microsoft.Owin.Security.Notifications;
using Microsoft.Owin.Host.SystemWeb;
using Trianz.Enterprise.Operations.General;

[assembly: OwinStartup(typeof(Trianz.Enterprise.Operations.Startup))]

namespace Trianz.Enterprise.Operations
{
	public class Startup
	{
		// The Client ID is used by the application to uniquely identify itself to Azure AD.
		string clientId = Environment.GetEnvironmentVariable("ClientId") ?? "";

		// RedirectUri is the URL where the user will be redirected to after they sign in.
		string redirectUri = Environment.GetEnvironmentVariable("RedirectUri") ?? "";

		// Tenant is the tenant ID (e.g. contoso.onmicrosoft.com, or 'common' for multi-tenant)
		static string tenant = Environment.GetEnvironmentVariable("Tenant") ?? "";

		// Authority is the URL for authority, composed by Microsoft identity platform endpoint and the tenant name (e.g. https://login.microsoftonline.com/contoso.onmicrosoft.com/v2.0)
		string authority = String.Format(System.Globalization.CultureInfo.InvariantCulture, Environment.GetEnvironmentVariable("Authority") ?? "", tenant);

		/// <summary>
		/// Configure OWIN to use OpenIdConnect 
		/// </summary>
		/// <param name="app"></param>
		public void Configuration(IAppBuilder app)
		{
			app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

			//app.UseCookieAuthentication(new CookieAuthenticationOptions());
			app.UseCookieAuthentication(new CookieAuthenticationOptions
			{
				// ...
				//AuthenticationType = "Cookies",
				//CookieManager = new Microsoft.Owin.Host.SystemWeb.SystemWebChunkingCookieManager()
				CookieManager = new SystemWebCookieManager()
			});
			app.UseOpenIdConnectAuthentication(
			new OpenIdConnectAuthenticationOptions
			{
				// Sets the ClientId, authority, RedirectUri as obtained from web.config
				ClientId = clientId,
				Authority = authority,
				RedirectUri = redirectUri,
				// PostLogoutRedirectUri is the page that users will be redirected to after sign-out. In this case, it is using the home page
				PostLogoutRedirectUri = redirectUri,
				Scope = OpenIdConnectScope.OpenIdProfile,
				// ResponseType is set to request the id_token - which contains basic information about the signed-in user
				ResponseType = OpenIdConnectResponseType.IdToken,
				// ValidateIssuer set to false to allow personal and work accounts from any organization to sign in to your application
				// To only allow users from a single organizations, set ValidateIssuer to true and 'tenant' setting in web.config to the tenant name
				// To allow users from only a list of specific organizations, set ValidateIssuer to true and use ValidIssuers parameter 
				TokenValidationParameters = new TokenValidationParameters()
				{
					ValidateIssuer = false
				},
				// OpenIdConnectAuthenticationNotifications configures OWIN to send notification of failed authentications to OnAuthenticationFailed method
				Notifications = new OpenIdConnectAuthenticationNotifications
				{
					AuthenticationFailed = OnAuthenticationFailed
				}
			}
		);
		}

		/// <summary>
		/// Handle failed authentication requests by redirecting the user to the home page with an error in the query string
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		private Task OnAuthenticationFailed(AuthenticationFailedNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions> context)
		{
			// skip IDX21323 exception
			if (context.Exception.Message.Contains("IDX21323"))
			{
				context.SkipToNextMiddleware();
			}
			else
			{
				context.HandleResponse();
				Common.WriteErrorLog("AuthenticationFailure" + context.Exception.Message);
				context.Response.Redirect("/?errormessage=" + context.Exception.Message);
			}
			return Task.FromResult(0);
		}
	}
}