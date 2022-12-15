using Dsf.Service.Template.Data.Models;
using Dsf.Service.Template.Resources;
using FluentValidation;
using Dsf.Service.Template.Services;

namespace Dsf.Service.Template.Data.Validations
{
    public class EmailValidator : AbstractValidator<EmailSection>
    {
        readonly ICommonApis _checker;
        readonly IResourceViewlocalizer _Localizer;
        public const string EmailExpression = @"^(([^<>()[\]\\.,;:\s@\""]+(\.[^<>()[\]\\.,;:\s@\""]+)*)|(\"".+\""))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$";
        string EmailNumNotFoundMsg = string.Empty;
        string EmailNoSelectionMsg = string.Empty;
        string EmailMessage = String.Empty;
        public EmailValidator(IResourceViewlocalizer localizer, ICommonApis checker)
        {
            _checker = checker;
            _Localizer = localizer;
            EmailNumNotFoundMsg = _Localizer["email-selection.no_results_check"];
            EmailNoSelectionMsg = _Localizer["email-selection.require_check"];
            EmailMessage = _Localizer["set-email.require_check"];
            When(p => p.validation_mode.Equals(ValidationMode.Select), () =>
            {
                //Selection page
                //RuleFor(x => x.email).NotEmpty().NotNull().When(x => x.use_from_api.Equals(true)).WithMessage(EmailNumNotFoundMsg);
                RuleFor(x => x.use_from_api).Equal(true).When(x => x.use_other.Equals(false)).WithMessage(EmailNoSelectionMsg);
            });
            //Edit page
            When(p => p.validation_mode.Equals(ValidationMode.Edit), () =>
            {
                RuleFor(p => p.email)
                                        .Cascade(CascadeMode.Stop)
                                        .NotEmpty().WithMessage(EmailMessage)
                                        .Must(_checker.IsEmailValid).WithMessage(EmailMessage);
            });
 
        }

    }

}
