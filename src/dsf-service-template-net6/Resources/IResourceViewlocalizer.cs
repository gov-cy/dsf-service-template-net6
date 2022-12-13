﻿using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;

namespace Dsf.Service.Template.Resources
{
    public interface IResourceViewlocalizer
    {
        //Keys in all files must be unique
        public LocalizedString this[string key]
        {
            get;
        }
       
        LocalizedString GetPagesLocalizedString(string key) ;
        LocalizedHtmlString GetPagesLocalizedHtml(string key);
        LocalizedHtmlString GetPagesLocalizedHtml(string key, params object[] arguments);

        LocalizedString GetErrorLocalizedString(string key);

        LocalizedString GetCommonLocalizedString(string key);
        LocalizedHtmlString GetCommonLocalizedHtml(string key);
    }
}
