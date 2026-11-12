using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Session;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OfficeOpenXml;
using System;
using System.Configuration;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using Trianz.Enterprise.Operations.Filters;
using Trianz.Enterprise.Operations.General;
using Trianz.Enterprise.Operations.Models;

var builder = WebApplication.CreateBuilder(args);

// Load configuration from appsettings.json
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

// Also load app.config for System.Configuration.ConfigurationManager compatibility
var configFile = System.Configuration.ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
if (configFile != null && configFile.AppSettings.Settings.Count > 0)
{
    foreach (string key in configFile.AppSettings.Settings.AllKeys)
    {
        if (builder.Configuration[key] == null)
        {
            builder.Configuration[key] = configFile.AppSettings.Settings[key].Value;
        }
    }
}

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    // Global filters (replaces FilterConfig.RegisterGlobalFilters)
    options.Filters.Add(new HandleErrorAttribute());
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
    options.Filters.Add<AuthorizeActionFilterAttribute>();
});

// Add session (replaces sessionState in web.config)
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add response compression
builder.Services.AddResponseCompression();

// Add antiforgery (replaces AntiForgeryConfig in Global.asax)
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
});

// Add authentication (replaces OWIN/Startup.cs)
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "Cookies";
    options.DefaultChallengeScheme = "OpenIdConnect";
})
.AddCookie("Cookies", options =>
{
    options.LoginPath = "/Login/Index";
    options.LogoutPath = "/Signout/Index";
    options.AccessDeniedPath = "/Error/NotAuthorized";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
    options.Cookie.Name = "TR-Operations";
})
.AddOpenIdConnect("OpenIdConnect", options =>
{
    options.ClientId = builder.Configuration["AppSettings:ClientId"];
    options.Authority = string.Format(builder.Configuration["AppSettings:Authority"], builder.Configuration["AppSettings:Tenant"]);
    options.CallbackPath = builder.Configuration["AppSettings:redirectUri"];
    options.ResponseType = "id_token";
    options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.TokenValidationParameters.ValidateIssuer = false;
    options.SaveTokens = true;
    options.GetClaimsFromUserInfoEndpoint = true;
    options.Events = new Microsoft.AspNetCore.Authentication.OpenIdConnect.OpenIdConnectEvents
    {
        OnAuthenticationFailed = context =>
        {
            Common.WriteErrorLog("AuthenticationFailure" + context.Exception.Message);
            context.HandleResponse();
            context.Response.Redirect("/?errormessage=" + context.Exception.Message);
            return Task.CompletedTask;
        }
    };
});

// Add EF Core DbContext (replaces EF6 entityFramework config in web.config)
builder.Services.AddDbContext<TrianzOperationsEntities>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("TrianzOperationsEntities")));

builder.Services.AddDbContext<TZ_CorporateOperationsEntities>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("TrianzOperationsEntities")));

// Add IHttpContextAccessor for controllers that need it
builder.Services.AddHttpContextAccessor();

// Add HttpClient
builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error/Error");
    app.UseStatusCodePagesWithRedirects("/Error/NotFound");
}

// Security headers middleware (replaces Application_PreSendRequestHeaders in Global.asax)
app.Use(async (context, next) =>
{
    context.Response.Headers.Remove("Server");
    context.Response.Headers.Remove("X-Powered-By");
    context.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubdomains=always");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Download-Options", "noopen");
    context.Response.Headers.Add("Content-Security-Policy", "'self' https://ajax.googleapis.com");
    await next();
});

app.UseResponseCompression();
app.UseStaticFiles();
app.UseRouting();

// HTTP method restrictions (replaces requestFiltering in web.config)
app.Use(async (context, next) =>
{
    var method = context.Request.Method;
    if (method == "OPTIONS" || method == "TRACE" || method == "HEAD")
    {
        context.Response.StatusCode = 405; // Method Not Allowed
        return;
    }
    await next();
});

app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

// Configure routing (replaces RouteConfig.RegisterRoutes)
app.MapControllerRoute(
    name: "Default",
    pattern: "{controller=TRHome}/{action=Index}/{id?}",
    defaults: new { controller = "TRHome", action = "Index", id = (string)null }
);

// Set EPPlus license context
ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

app.Run();
