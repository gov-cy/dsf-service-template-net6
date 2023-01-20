using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DSF.AspNetCore.Web.Template.Pages
{
    public class ServerErrorModel : PageModel
    {
        private readonly ILogger<ServerErrorModel> _logger;

        public ServerErrorModel(ILogger<ServerErrorModel> logger) 
        {
            _logger = logger;
        }

        public void OnGet()
        {
            HttpContext.Session.Clear();

            // Get the details of the exception that occurred
            var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            if (exceptionFeature != null)
            {
                // Get which route the exception occurred at
                string routeWhereExceptionOccurred = exceptionFeature.Path;

                // Get the exception that occurred
                Exception exceptionThatOccurred = exceptionFeature.Error;

                // TODO: Do something with the exception
                // Log it with a logging provider
                _logger.LogError(routeWhereExceptionOccurred, exceptionThatOccurred);
                // Whatever you do, be careful to catch any exceptions, otherwise you'll end up with a blank page and throwing a 500
            }
        }
    }
}
