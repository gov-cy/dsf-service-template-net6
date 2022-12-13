using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;

namespace Dsf.Service.Template.Middlewares
{
    public static class RequestLocalizationCookiesMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestLocalizationCookies(this IApplicationBuilder app)
        {
            app.UseMiddleware<RequestLocalizationCookiesMiddleware>();
            return app;
        }
    } 
    
    public class RequestLocalizationCookiesMiddleware : IMiddleware
    {
        public CookieRequestCultureProvider Provider { get; }

        public RequestLocalizationCookiesMiddleware(IOptions<RequestLocalizationOptions> requestLocalizationOptions)
        {
            Provider =
                requestLocalizationOptions
                    .Value
                    .RequestCultureProviders
                    .Where(x => x is CookieRequestCultureProvider)
                    .Cast<CookieRequestCultureProvider>()
                    .FirstOrDefault();
        }
        
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                if (Provider != null)
                {
                    var feature = context.Features.Get<IRequestCultureFeature>();

                    if (feature != null)
                    {
                        // remember culture across request
                        context.Response
                            .Cookies
                            .Append(
                                Provider.CookieName,
                                CookieRequestCultureProvider.MakeCookieValue(feature.RequestCulture)
                            );
                    }
                }
            }
            catch
            {

            }
            

            await next(context);
        }
    }
}