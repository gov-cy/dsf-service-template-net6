using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Globalization;

namespace DSF.AspNetCore.Web.Template.ViewComponents
{
    public class CulturePicker : ViewComponent
    {
        private readonly IOptions<RequestLocalizationOptions> localizationOptions;

        public CulturePicker(IOptions<RequestLocalizationOptions> localizationOptions)
        {
            this.localizationOptions = localizationOptions;
        }

        public IViewComponentResult Invoke()
        {
            var cultureFeature = HttpContext.Features.Get<IRequestCultureFeature>();
            var model = new CulturePickerModel
            {
                SupportedCultures = localizationOptions.Value.SupportedUICultures?.ToList() ?? new(),
            };

            if (cultureFeature != null)
            {
                model.CurrentUICulture = cultureFeature.RequestCulture.UICulture;
            }
            return View(model);
        }
    }

    public class CulturePickerModel
    {
        public CultureInfo? CurrentUICulture { get; set; }
        public List<CultureInfo> SupportedCultures { get; set; } = new();
    }
}