using dsf_service_template_net6.Data.Models;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace dsf_service_template_net6.Data.Validations
{
    public class cEmailEditValidator : AbstractValidator<EmailEdit>
    {
        IStringLocalizer _Localizer;
        public cEmailEditValidator(IStringLocalizer localizer)
        {
            _Localizer = localizer;
        }
    }
}
