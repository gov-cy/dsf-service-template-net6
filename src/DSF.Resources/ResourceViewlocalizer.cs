using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace DSF.Localization
{
    public class ResourceViewLocalizer : IResourceViewLocalizer

    {
        private readonly IHtmlLocalizer htmlLocalizerPages;
        private readonly IStringLocalizer localizerPages;
        private readonly IStringLocalizer localizerError;
        private readonly IStringLocalizer localizerCommon;
        private readonly IHtmlLocalizer htmlLocalizerCommon;
        public ResourceViewLocalizer(IOptions<ResourceOptions> pageResource, IStringLocalizerFactory factory, IHtmlLocalizerFactory htmlFactory)
        {
            if (pageResource.Value.PageResourceLocationByType == null) throw new ArgumentNullException($"{pageResource.Value.PageResourceLocationByType} must be defined!");
            if (pageResource.Value.CommonResourceLocationByType == null) throw new ArgumentNullException($"{pageResource.Value.CommonResourceLocationByType} must be defined!");
            if (pageResource.Value.ErrorResourceLocationByType == null) throw new ArgumentNullException($"{pageResource.Value.ErrorResourceLocationByType} must be defined!");

            var pageAssembly = new AssemblyName(pageResource.Value.PageResourceLocationByType.GetTypeInfo().Assembly.FullName!);

            var errorAssembly = new AssemblyName(pageResource.Value.ErrorResourceLocationByType.GetTypeInfo().Assembly.FullName!);

            var commonAssembly = new AssemblyName(pageResource.Value.CommonResourceLocationByType.GetTypeInfo().Assembly.FullName!);

            localizerPages = factory.Create(pageResource.Value.PageResourceLocationByType.Name, pageAssembly.Name!);
            htmlLocalizerPages = htmlFactory.Create(pageResource.Value.PageResourceLocationByType.Name, pageAssembly.Name!);

            localizerError = factory.Create(pageResource.Value.ErrorResourceLocationByType.Name, errorAssembly.Name!);

            localizerCommon = factory.Create(pageResource.Value.CommonResourceLocationByType.Name, commonAssembly.Name!);
            htmlLocalizerCommon = htmlFactory.Create(pageResource.Value.CommonResourceLocationByType.Name, commonAssembly.Name!);
        }
        public LocalizedString this[string key] => (!localizerCommon[key].ResourceNotFound) 
            ? localizerCommon[key] 
            : (!localizerError[key].ResourceNotFound) 
                ? localizerError[key] 
                : localizerPages[key];

        public LocalizedString GetErrorLocalizedString(string key)
        {
            return localizerError[key];
        }
        public LocalizedString GetPagesLocalizedString(string key)
        {
            return localizerPages[key];
        }
        public LocalizedHtmlString GetPagesLocalizedHtml(string key)
        {
            return htmlLocalizerPages[key];
        }
        public LocalizedHtmlString GetPagesLocalizedHtml(string key, params object[] arguments)
        {
            return htmlLocalizerPages.GetHtml(key, arguments);
        }
        public LocalizedString GetCommonLocalizedString(string key)
        {
            return localizerCommon[key];
        }
        public LocalizedHtmlString GetCommonLocalizedHtml(string key)
        {
            return htmlLocalizerCommon[key];
        }
    }
}