using FluentValidation;
using ClaimsModule.Domain.Enumerations;

namespace ClaimsModule.Application.Features.Claims.Commands.AddClaimParty;

public class AddClaimPartyCommandValidator : AbstractValidator<AddClaimPartyCommand>
{
    public AddClaimPartyCommandValidator()
    {
        RuleFor(v => v.ClaimId)
            .NotEmpty().WithMessage("Claim ID is required.");

        RuleFor(v => v.PartyRole)
            .NotEmpty().WithMessage("Party role is required.")
            .IsEnumName(typeof(PartyRole), false).WithMessage("Invalid party role.");

        RuleFor(v => v.PartyType)
            .NotEmpty().WithMessage("Party type is required.")
            .IsEnumName(typeof(PartyType), false).WithMessage("Invalid party type.");

        RuleFor(v => v.FirstName)
            .NotEmpty().WithMessage("First name is required for person party.")
            .When(v => v.PartyType == "Person");

        RuleFor(v => v.LastName)
            .NotEmpty().WithMessage("Last name is required for person party.")
            .When(v => v.PartyType == "Person");

        RuleFor(v => v.CompanyName)
            .NotEmpty().WithMessage("Company name is required for company party.")
            .When(v => v.PartyType == "Company");

        RuleFor(v => v.Email)
            .EmailAddress().WithMessage("A valid email address is required.")
            .When(v => !string.IsNullOrEmpty(v.Email));
    }
}
