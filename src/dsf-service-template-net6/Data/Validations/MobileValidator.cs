using dsf_moi_election_catalogue.Services;
using Dsf.Service.Template.Data.Models;
using Dsf.Service.Template.Resources;
using FluentValidation;

namespace Dsf.Service.Template.Data.Validations
{
    public class MobileValidator : AbstractValidator<MobileSection>
    {
        readonly ICommonApis _checker;
        readonly IResourceViewlocalizer _Localizer;
        string MobileNoSelectionMsg = string.Empty;
        public const string Expression = @"^[1-9]\d*(\.\d+)?$";
        string mobReq = string.Empty;
        string mobValid = string.Empty;
        public MobileValidator(IResourceViewlocalizer localizer, ICommonApis commonApis)
        {
            _checker=commonApis;
            _Localizer = localizer;
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
                 .Matches("[0-9]{8,15}$").WithMessage(mobValid)
                 .Must(_checker.IsMobileValid).WithMessage(mobValid); 
            });
        }
    }    
}
