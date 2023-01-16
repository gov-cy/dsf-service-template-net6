using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Dsf.Service.Template.ViewComponents
{
    public class CulturePicker : ViewComponent
    {
       private readonly  IOptions<RequestLocalizationOptions> localizationOptions;

        public   CulturePicker(IOptions<RequestLocalizationOptions> localizationOptions)
        {
            this.localizationOptions = localizationOptions;
        }

        public IViewComponentResult Invoke()
        {
            var cultureFeature = HttpContext.Features.Get<IRequestCultureFeature>();
            RequestLocalizationOptions value = localizationOptions!.Value;
            var model = new CulturePickerModel
            {
                SupportedCultures = value!.SupportedUICultures!.ToList(),
                CurrentUICulture = cultureFeature!.RequestCulture.UICulture
            };

            return View(model);
        }
    }

    public class CulturePickerModel
    {
        public CultureInfo? CurrentUICulture { get; set; }
        public List<CultureInfo>? SupportedCultures { get; set; }
        
       
    }
}