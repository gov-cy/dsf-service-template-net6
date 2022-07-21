using dsf_service_template_net6.Data.Models;
using dsf_service_template_net6.Data.Validations;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;

namespace dsf_service_template_net6.Pages
{
    [BindProperties]
    public class EmailEditModel : PageModel
    {
        private IValidator<EmailEdit> _validator;
        IStringLocalizer _Loc;
        public string displaySummary = "display:none";
        public string ErrorsDesc = "";
        public string MobileErrorClass = "";
        public EmailEdit emailEdit { get; set; }
        public EmailEditModel(IValidator<EmailEdit> validator, IStringLocalizer<cEmailEditValidator> Loc)
        {

            _validator = validator;
            _Loc = Loc;
            emailEdit = new EmailEdit();
        }
        void ClearErrors()
        {
            displaySummary = "display:none";
            MobileErrorClass = "";
            ErrorsDesc = "";
        }
        public void OnGet()
        {
        }
        public IActionResult OnPostSetEmail(bool review)
        {
            //Finally redirect
            return RedirectToPage("/ReviewPage");
        }
    }
}
