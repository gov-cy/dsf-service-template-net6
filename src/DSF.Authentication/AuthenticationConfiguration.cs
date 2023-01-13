using DSF.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace DSF.MOI.CitizenData.Web.Configuration
{
    public static class AuthenticationConfiguration
    {
        public const string OIDCScheme = "Oidc";
        public const string DsfCyLoginAuthCookie = "DsfCyLoginAuthCookie";

        private static IWebHostEnvironment? _environment;
        private static ICyLoginSpecification? _cyLoginSpecification;
        private static IConfiguration? _configSection;

        public static void UseCyLoginAuthentication(this WebApplication? app) 
        {
            if(app == null) throw new ArgumentNullException(nameof(app));

            if (app.Configuration.GetChildren().Any(x => x.Key == OIDCScheme))
            {
                _configSection = app.Configuration.GetSection(OIDCScheme);
            }
            else 
            {
                throw new ArgumentException($"Configuration section {OIDCScheme} not found.");
            }
           
            _environment = app.Environment;
            _cyLoginSpecification = app?.Services.GetRequiredService<ICyLoginSpecification>();
        }

        public static void AddCyLoginAuthentication(this IServiceCollection services)
        {
            services.Configure<OIDCSettings>(options =>
            {
                options = _configSection.Get<OIDCSettings>();
            });

            services.AddSingleton<ICyLoginSpecification, CyLoginSpecification>();

            services
                .AddOptions<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme)

                .Configure((options) =>
                {
                    var oidcSettings = _configSection.Get<OIDCSettings>();

                    options.Authority = oidcSettings.Authority;
                    options.ClientId = oidcSettings.ClientId;
                    options.ClientSecret = oidcSettings.ClientSecret;

                    options.Events.OnTicketReceived = ctx =>
                    {
                        ctx.ReturnUri = oidcSettings.LoginUrl;
                        return Task.CompletedTask;
                    };

                    if (!string.IsNullOrEmpty(oidcSettings.SignedOutRedirectUri))
                    {
                        options.SignedOutRedirectUri = oidcSettings.SignedOutRedirectUri;
                    }

                    //Split the scopes and add them
                    //Make sure to clear scopes first to remove any defaults
                    options.Scope.Clear();
                    if (!string.IsNullOrEmpty(oidcSettings.Scopes))
                    {
                        Array.ForEach
                        (
                            oidcSettings.Scopes.Split(" ", StringSplitOptions.RemoveEmptyEntries),
                            str => options.Scope.Add(str)
                        );
                    }
                });

            services
                .AddAuthentication(options =>
                {
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme; //"OpenIdConnect";
                })
                .AddCookie(options =>
                {
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                    options.Cookie.SameSite = SameSiteMode.Lax;
                    options.Cookie.Name = DsfCyLoginAuthCookie;
                    options.SlidingExpiration = true;
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
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

                    if (_environment.IsDevelopment())
                    {
                        //TODO - remove for production - not for production
                        HttpClientHandler handler = new();
                        handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
                        options.BackchannelHttpHandler = handler;
                    }

                    options.ResponseType = "code";
                    options.ResponseMode = "query";

                    options.SaveTokens = true;
                    options.UsePkce = true;
                    options.GetClaimsFromUserInfoEndpoint = true;

                    _cyLoginSpecification?.SetClaims(options.ClaimActions);

                    options.Events.OnRemoteFailure = ctx =>
                    {
                        //Handle exception when user clicks deny (No, Do Not Allow) on CyLogin Page
                        ctx.Response.Redirect("/");
                        ctx.HandleResponse();
                        return Task.FromResult(0);
                    };
                    options.Events.OnRedirectToIdentityProvider = ctx =>
                    {
                        ctx.ProtocolMessage.UiLocales = Thread.CurrentThread.CurrentUICulture.Name;
                        return Task.CompletedTask;
                    };
                });
        }
    }
}
