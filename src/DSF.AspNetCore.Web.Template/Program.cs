using DSF.AspNetCore.Web.Template.Data.Models;
using DSF.AspNetCore.Web.Template.Data.Validations;
using DSF.AspNetCore.Web.Template.Extensions;
using DSF.AspNetCore.Web.Template.Middlewares;
using DSF.AspNetCore.Web.Template.Resources;
using DSF.AspNetCore.Web.Template.Services;
using DSF.AspNetCore.Web.Template.Services.UserSatisfaction;
using DSF.Localization;
using DSF.MOI.CitizenData.Web.Configuration;
using FluentValidation;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using System.IdentityModel.Tokens.Jwt;

IConfiguration Configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
    .AddJsonFile("secrets/appsettings.json", optional: true, reloadOnChange: true)
    .AddUserSecrets<Program>(true)
    .Build();

var builder = WebApplication.CreateBuilder(args);

IWebHostEnvironment environment = builder.Environment;
JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

//Localization Configuration
builder.Services.AddLocalization(o => o.ResourcesPath = "Resources");

builder.Services.AddAntiforgery(options =>
{
    // Set Cookie properties using CookieBuilder properties.
    options.FormFieldName = "DsfAftNetVR";
    options.HeaderName = "X-CSRF-TOKEN-HEADER";
    options.SuppressXFrameOptionsHeader = false;
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.SetDefaultCulture("el-GR");
    options.AddSupportedUICultures("el-GR", "en-GB");
    options.FallBackToParentUICultures = true;
    options.RequestCultureProviders.Remove(typeof(AcceptLanguageHeaderRequestCultureProvider));
});

//controller addition
builder.Services.AddControllers();
// Configure authorize pages.
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizePage("/Navigation");
    options.Conventions.AuthorizePage("/Email");
    options.Conventions.AuthorizePage("/Mobile");
    options.Conventions.AuthorizePage("/Address");
    options.Conventions.AuthorizePage("/EmailEdit");
    options.Conventions.AuthorizePage("/MobileEdit");
    options.Conventions.AuthorizePage("/AddressEdit");
    options.Conventions.AuthorizePage("/ReviewPage");
    options.Conventions.AuthorizePage("/UserSatisfaction");
    options.Conventions.AuthorizePage("/UserSatisfactionResponse");
    options.Conventions.AllowAnonymousToPage("/NoValidProfile");
    options.Conventions.AllowAnonymousToPage("/Index");
    options.Conventions.AllowAnonymousToPage("/CookiePolicy");
    options.Conventions.AllowAnonymousToPage("/AccessibilityStatement");
    options.Conventions.AllowAnonymousToPage("/PrivacyStatement");
    options.Conventions.AllowAnonymousToPage("/NoPageFound");
}).AddViewLocalization();
builder.Services.AddSingleton<IResourceViewLocalizer, ResourceViewLocalizer>();
builder.Services.Configure<ResourceOptions>(options =>
{
    options.ErrorResourceLocationByType = typeof(ErrorResource);
    options.PageResourceLocationByType = typeof(PageResource);
    options.CommonResourceLocationByType = typeof(CommonResource);
});
//Register Validator for
//dependency injection purpose
//for resource access
//for server side validations
builder.Services.AddScoped<IValidator<EmailSection>, EmailValidator>(sp =>
{
    var LocMain = sp.GetRequiredService<IResourceViewLocalizer>();
    var Checker = sp.GetRequiredService<ICommonApis>();
    return new EmailValidator(LocMain, Checker);
});
builder.Services.AddScoped<IValidator<MobileSection>, MobileValidator>(sp =>
{
    var LocMain = sp.GetRequiredService<IResourceViewLocalizer>();
    var Checker = sp.GetRequiredService<ICommonApis>();
    return new MobileValidator(LocMain, Checker);
});
builder.Services.AddScoped<IValidator<UserSatisfactionViewModel>, UserSatisfactionValidation>(sp =>
{
    var LocMain = sp.GetRequiredService<IResourceViewLocalizer>();
    return new UserSatisfactionValidation(LocMain);
});

//Add fluent validation to .Net Core (optional use for server side validation) 
//builder.Services.AddFluentValidation();
//multi language support localization middleware 
builder.Services.AddScoped<RequestLocalizationCookiesMiddleware>();

//IHttpContextAccessor register
builder.Services.AddHttpContextAccessor();

//Register HttpClient
//so that it can be used for Dependency Injection
//for calling all http client requests 
builder.Services.AddSingleton<IMyHttpClient, MyHttpClient>();
builder.Services.AddSingleton<ICommonApis, CommonApis>();

//Register Navigation Service
builder.Services.AddScoped<INavigation, Navigation>();

//Register Session Service
builder.Services.AddScoped<IUserSession, UserSession>();

//Register the Api service for Task Get and post methods
builder.Services.AddScoped<IContact, Contact>();
builder.Services.AddScoped<IUserSatisfactionService, UserSatisfactionService>();

//get configuration for CyLoginAuthentication from appsettings.json
var authConfiguration = Configuration.GetSection("Dsf.Authentication").Get<CyLoginAuthenticationOptions>();
//open id authentication settings
builder.Services.AddCyLoginAuthentication(authConfiguration);

// Added for session state
builder.Services.AddSession(options =>
 {
     options.Cookie.Name = "AppDataSessionCookie";
     options.Cookie.HttpOnly = true;
     options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
     options.Cookie.IsEssential = true;
     options.IdleTimeout = TimeSpan.FromMinutes(double.Parse(authConfiguration.ExpireTimeSpanInMinutes ?? "30")); //same with authentication idle time
 });
 var app = builder.Build();

// Configure the HTTP request pipeline middlewares.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseExceptionHandler("/server-error");
    app.UseHsts();
}
// This is needed if running behind a reverse proxy (K8S Ingres?)
//https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/proxy-load-balancer?view=aspnetcore-5.0
var options = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};
options.KnownNetworks.Clear();
options.KnownProxies.Clear();

app.UseForwardedHeaders(options);
app.UseHttpsRedirection();

//IdentityServer4 http to https error (Core 1,2,3,5,6)
app.Use(async (ctx, next) =>
{
    ctx.Request.Scheme = "https";


    await next();
});

app.UseCors();

app.UseStaticFiles();

app.UseRequestLocalization();

// will remember to write the cookie 
app.UseRequestLocalizationCookies();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

app.MapRazorPages();
app.UseStatusCodePagesWithRedirects("/no-page-found");
app.UseEndpoints(endpoints =>
{
    endpoints.MapRazorPages();
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Account}/{action=Index}/{id?}");

});

app.Run();
