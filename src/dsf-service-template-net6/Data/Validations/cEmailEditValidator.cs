using dsf_service_template_net6.Data.Models;
using dsf_service_template_net6.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace dsf_service_template_net6.Data.Validations
{
    public class cEmailEditValidator : AbstractValidator<EmailEdit>
    {
        IResourceViewlocalizer _Localizer;
        public const string EmailExpression = @"^(([^<>()[\]\\.,;:\s@\""]+(\.[^<>()[\]\\.,;:\s@\""]+)*)|(\"".+\""))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$";
        string EmailMessage = string.Empty;
        string EmailAlreadyExists=string.Empty;
        

        public cEmailEditValidator(IResourceViewlocalizer localizer)
        {
            _Localizer = localizer;
            EmailMessage = _Localizer["EmailRequired"];
            EmailAlreadyExists= _Localizer["EmailNumberAlreadyExists"];
            RuleFor(p => p.email)
                    .Cascade(CascadeMode.Stop)
                    .NotEmpty().WithMessage(EmailMessage)
                    .NotEqual(x => x.prev_email).WithMessage(EmailAlreadyExists)
                    .Matches(EmailExpression).WithMessage(EmailMessage);
            
        }
    }
}
