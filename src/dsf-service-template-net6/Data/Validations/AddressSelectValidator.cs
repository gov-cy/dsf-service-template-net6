using dsf_service_template_net6.Data.Models;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace dsf_service_template_net6.Data.Validations
{
    public class AddressSelectValidator : AbstractValidator<AddressSelect>
    {
        IStringLocalizer _Localizer;
        string AddressNotFoundMsg = string.Empty;
        string AddressNoSelectionMsg = string.Empty;
        public AddressSelectValidator(IStringLocalizer localizer)
        {

            _Localizer = localizer;
            AddressNotFoundMsg = _Localizer["AddressNotFound"];
            AddressNoSelectionMsg = _Localizer["AdressSelection"];
            RuleFor(x => x.addressInfo)
                .Must(x => x.Count()> 0).When(x => x.use_other == false).WithMessage(AddressNotFoundMsg);
            RuleForEach(x=>x.addressInfo ).ChildRules(addressInfo => 
            { 
                addressInfo.RuleFor(x=>x.addressText ).NotEmpty().NotNull();
                addressInfo.RuleFor(x=> x.postalCode).NotEmpty().NotNull();
                addressInfo.RuleFor(x => x.item.code).NotEmpty().NotNull();
                addressInfo.RuleFor(x => x.parish.code).NotEmpty().NotNull();
                addressInfo.RuleFor(x => x.district.code).NotEmpty().NotNull();
                addressInfo.RuleFor(x => x.town.code).NotEmpty().NotNull();
            }).When(x=> x.use_other==false).WithMessage(AddressNotFoundMsg);
            RuleFor(x => x.use_from_civil).Equal(true).When(x => x.use_other.Equals(false)).WithMessage(AddressNoSelectionMsg);
        }

    }
}
