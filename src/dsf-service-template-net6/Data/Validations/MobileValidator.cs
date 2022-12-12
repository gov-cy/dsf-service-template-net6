using dsf_service_template_net6.Data.Models;
using dsf_service_template_net6.Resources;
using FluentValidation;

namespace dsf_service_template_net6.Data.Validations
{
    public class MobileValidator : AbstractValidator<MobileSection>
    {
        readonly IResourceViewlocalizer _Localizer;
        string MobileNumNotFoundMsg = string.Empty;
        string MobileNoSelectionMsg = string.Empty;
        public const string Expression = @"^[1-9]\d*(\.\d+)?$";
        string mobReq = string.Empty;
        string mobValid = string.Empty;
        public MobileValidator(IResourceViewlocalizer localizer)
        {
            _Localizer = localizer;
            MobileNumNotFoundMsg = _Localizer["MobileNotFound"];
            MobileNoSelectionMsg = _Localizer["mobile-selection.require_check"];
            mobReq = _Localizer["set-mobile.require_check"];
            mobValid = _Localizer["set-mobile.format_check"];
            When(p => p.validation_mode.Equals(ValidationMode.Select), () =>
            {
                RuleFor(x => x.use_from_api).Equal(true).When(x => x.use_other.Equals(false)).WithMessage(MobileNoSelectionMsg);
            });
            //Edit Mobile
            When(p => p.validation_mode.Equals(ValidationMode.Edit), () =>
            {
                RuleFor(p => p.mobile)
                 .Cascade(CascadeMode.Stop)
                 .NotEmpty().WithMessage(mobReq)
                .Matches(Expression).WithMessage(mobValid);
            });
        }
    }    
}
