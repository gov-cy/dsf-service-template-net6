using dsf_service_template_net6.Data.Models;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace dsf_service_template_net6.Data.Validations
{
    public class cEmailEditValidator : AbstractValidator<EmailEdit>
    {
        IStringLocalizer _Localizer;
        public const string EmailExpression = @"^(([^<>()[\]\\.,;:\s@\""]+(\.[^<>()[\]\\.,;:\s@\""]+)*)|(\"".+\""))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$";
        string EmailMessage = string.Empty;
        

        public cEmailEditValidator(IStringLocalizer localizer)
        {
            _Localizer = localizer;
            EmailMessage = _Localizer["EmailRequired"];
            RuleFor(p => p.email)
                    .Cascade(CascadeMode.Stop)
                    .NotEmpty().WithMessage(EmailMessage)
                    .Matches(EmailExpression).WithMessage(EmailMessage);
            
        }
    }
}
