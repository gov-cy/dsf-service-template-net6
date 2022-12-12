using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using System.IdentityModel.Tokens.Jwt;
using dsf_service_template_net6.Extensions;
using dsf_service_template_net6.Middlewares;
using dsf_service_template_net6.Services;
using dsf_service_template_net6.Resources;
using dsf_service_template_net6.Data.Models;
using dsf_service_template_net6.Data.Validations;
using FluentValidation;
using dsf_moi_election_catalogue.Services;

IConfiguration Configuration = new ConfigurationBuilder()
                            .AddJsonFile("appsettings.json")
                            .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
                            .AddJsonFile("secrets/appsettings.json", optional: true, reloadOnChange: true)
                            .AddUserSecrets<Program>(true)
                            .Build();

var builder = WebApplication.CreateBuilder(args);
IWebHostEnvironment environment = builder.Environment;
JwtSecurityTokenHandler.DefaultMapInboundClaims = false;
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
//Localization Configuration
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
    options.Conventions.AllowAnonymousToPage("/NoValidProfile");
    options.Conventions.AllowAnonymousToPage("/Index");
    options.Conventions.AllowAnonymousToPage("/CookiePolicy");
    options.Conventions.AllowAnonymousToPage("/AccessibilityStatement");
    options.Conventions.AllowAnonymousToPage("/PrivacyStatement");
}).AddViewLocalization();
//Register Validator for
//dependency injection purpose
//for resource access
builder.Services.AddSingleton<IResourceViewlocalizer, ResourceViewlocalizer>();
//for server side validations
builder.Services.AddScoped<IValidator<EmailSection>, EmailValidator>(sp =>
{
    var LocMain = sp.GetRequiredService<IResourceViewlocalizer>();
    var Checker = sp.GetRequiredService<ICommonApis>();
    return new EmailValidator(LocMain, Checker);
});
builder.Services.AddScoped<IValidator<MobileSection>, MobileValidator>(sp =>
{
    var LocMain = sp.GetRequiredService<IResourceViewlocalizer>();
    var Checker = sp.GetRequiredService<ICommonApis>();
    return new MobileValidator(LocMain, Checker);
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
//Added for session state
builder.Services.AddSession(options => {
    options.Cookie.Name = "AppDataSessionCookie";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromMinutes(30);
});
//open id authentication settings
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme; //"oidc";
})
.AddCookie(options =>
{ 
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.Name = "DsfCyLoginAuthCookie";
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    //options.Cookie.MaxAge = options.ExpireTimeSpan;
 })
.AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
{
    //The ClientId and ClientSecret properties are initiated in user-secrets, not in appsettings.json
    //dotnet user-secrets init
    //dotnet user-secrets list
    //dotnet user-secrets set "Oidc:Authority" "https://my_authority_uri"
    //dotnet user-secrets set "Oidc:RedirectUri" "https://my_redirect_uri"
    //dotnet user-secrets set "Oidc:ClientSecret" "my_secret"
    //dotnet user-secrets set "Oidc:ClientId" "my_client_id"
    //dotnet user-secrets set "Oidc:Scopes" "my_scope1 my_scope2 ..."

    if (environment.IsDevelopment())
    {
        //TODO - remove for production - not for production
        using HttpClientHandler handler = new();
        handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
        options.BackchannelHttpHandler = handler;
    }

    options.Authority = Configuration["Oidc:Authority"];
    options.ClientId = Configuration["Oidc:ClientId"];
    options.ClientSecret = Configuration["Oidc:ClientSecret"];
    //options.SignedOutRedirectUri = Configuration["Oidc:RedirectUri"];

    //Split the scopes and add them
    //Make sure to clear scopes first to remove any defaults
    string[] scopes = Configuration["Oidc:Scopes"].Split(" ", System.StringSplitOptions.RemoveEmptyEntries);
    options.Scope.Clear();
    foreach (var s in scopes)
    {
        options.Scope.Add(s);
    }

    //Port used for this client MUST BE 44319
    //options.SignedOutRedirectUri = "https://localhost:44319/";
    
    options.ResponseType = "code";
    options.ResponseMode = "query";

    options.SaveTokens = true;
    options.UsePkce = true;
    options.GetClaimsFromUserInfoEndpoint = true;

    //Map custom fields
    //CY Login Specifications v2.1
    options.ClaimActions.MapJsonKey("unique_identifier", "unique_identifier");
    options.ClaimActions.MapJsonKey("legal_unique_identifier", "legal_unique_identifier");
    options.ClaimActions.MapJsonKey("legal_main_profile", "legal_main_profile");
     //EIDAS
    options.ClaimActions.MapJsonKey("given_name", "given_name");
    options.ClaimActions.MapJsonKey("family_name", "family_name");
    options.ClaimActions.MapJsonKey("birthdate", "birthdate");

    //Handle exception when user clicks deny (No, Do Not Allow) on CyLogin Page
    options.Events = new OpenIdConnectEvents
    {
        OnRemoteFailure = context =>
        {
            context.Response.Redirect("/");
            context.HandleResponse();

            return Task.FromResult(0);
        },
        OnTicketReceived = ctx =>
        {
            var url = "/Account/LogIn";
            ctx.ReturnUri = url;
            return Task.CompletedTask;
        }
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline middlewares.
if (!app.Environment.IsDevelopment())
{
    app.UseStatusCodePagesWithRedirects("/NoPageFound");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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

//IdentityServer4 http to https error (Core 1,2,3,5,6)
app.Use(async (ctx, next) =>
{
    ctx.Request.Scheme = "https";
    await next();
});

app.UseSession();

app.UseCors();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRequestLocalization();

// will remember to write the cookie 
app.UseRequestLocalizationCookies();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.UseEndpoints(endpoints =>
{
    endpoints.MapRazorPages();
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Account}/{action=Index}/{id?}");

});
app.Run();
