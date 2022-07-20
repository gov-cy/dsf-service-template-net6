using dsf_service_template_net6.Data.Models;
using FluentValidation;
using Microsoft.Extensions.Localization;
namespace dsf_service_template_net6.Data.Validations
{
    public class cMobileEditValidator: AbstractValidator<MobileEdit>
    {
        public const string Expression = @"^[1-9]\d*(\.\d+)?$";
        string mobReq = string.Empty;
        string mobAlreadyExists = string.Empty;
        string mobValid= string.Empty;
        IStringLocalizer _Localizer;
      public cMobileEditValidator(IStringLocalizer<cMobileEditValidator> localizer)
      {
            
            _Localizer = localizer;
            mobReq = _Localizer["MolibeNumberRequired"];
            mobAlreadyExists= _Localizer["MolibeNumberAlreadyExists"];
            mobValid = _Localizer["MobileNumberValid"];

            RuleFor(p => p.mobile)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage(mobReq)
                .NotEqual(x => x.prev_mobile).WithMessage(mobAlreadyExists)
                .Matches(Expression).WithMessage(mobValid); 
        }

    }
}
