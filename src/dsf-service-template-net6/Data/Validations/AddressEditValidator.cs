using dsf_service_template_net6.Data.Models;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace dsf_service_template_net6.Data.Validations
{
    public class AddressEditValidator : AbstractValidator<AddressEditViewModel>
    {
        IStringLocalizer _Localizer;
        string PostalRequiredMsg = string.Empty;
        public AddressEditValidator(IStringLocalizer localizer)
        {

            _Localizer = localizer;
            PostalRequiredMsg = _Localizer["PostalRequired"];
            RuleFor(x => x.postalCode)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(PostalRequiredMsg)
            .Length(4).WithMessage(_Localizer["PostCodeLength"])
            .Matches("^[0-9]{4}$").WithMessage(_Localizer["PostCodeValid"]);
            When(p => !string.IsNullOrEmpty(p.postalCode) && p.HasUserSelectedAddress==false && p.HasUserEnteredPostalCode == true, () =>
            {
                RuleFor(x => x.Addresses)
                .Cascade(CascadeMode.Stop)
                .NotNull().When(x=> x.postalCode.Length==4)
                .NotEmpty().When(x => x.postalCode.Length == 4);
            });

            When(p => !string.IsNullOrEmpty(p.postalCode) && !string.IsNullOrEmpty(p.SelectedAddress), () =>
            {
                RuleFor(address => address.StreetNo)
                    .Cascade(CascadeMode.Stop)
                    .NotEmpty()
                    .WithMessage(_Localizer["StreetNoMandatory"])
                     .MaximumLength(5)
                        .WithMessage(_Localizer["StreetNoLength"]);
                       
                    RuleFor(address => address.FlatNo)
                         
                        .MaximumLength(5).When(address => !string.IsNullOrEmpty(address.FlatNo))
                        .WithMessage(_Localizer["FlatNoLength"]);
            });
        }


    }
}
