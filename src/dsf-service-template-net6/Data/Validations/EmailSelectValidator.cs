using dsf_service_template_net6.Data.Models;
using dsf_service_template_net6.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace dsf_service_template_net6.Data.Validations
{
    public class EmailSelectValidator :AbstractValidator<EmailSelect>
    {
        IResourceViewlocalizer _Localizer;
        string EmailNumNotFoundMsg = string.Empty;
        string EmailNoSelectionMsg = string.Empty;
        public EmailSelectValidator(IResourceViewlocalizer localizer)
        {
            _Localizer = localizer;
            EmailNumNotFoundMsg = _Localizer["EmailNotFound"];
            EmailNoSelectionMsg = _Localizer["EmailSelection"];
            RuleFor(x => x.email).NotEmpty().NotNull().WithMessage(EmailNumNotFoundMsg);
            RuleFor(x => x.use_from_civil).Equal(true).When(x => x.use_other.Equals(false)).WithMessage(EmailNoSelectionMsg);
        }

    }
}
