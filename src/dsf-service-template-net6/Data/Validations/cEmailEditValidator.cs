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
        string EmailNotSelected = string.Empty;

        public cEmailEditValidator(IStringLocalizer localizer)
        {
            _Localizer = localizer;
            EmailMessage = _Localizer["EmailRequired"];
            EmailNotSelected = _Localizer["ChooseEmail"];
            RuleFor(p => p.otherEmail)
                    .Cascade(CascadeMode.Stop)
                    .NotEmpty().When(p => p.useOther.Equals(true)).WithMessage(EmailMessage)
                    .Matches(EmailExpression).When(p => p.useOther.Equals(true)).WithMessage(EmailMessage);
            RuleFor(p => p.useAriadni)
                     .Cascade(CascadeMode.Stop)
                     .Equal(true).When(p => p.useOther.Equals(false)).WithMessage(EmailNotSelected);
        }
    }
}
