using FluentValidation;

namespace ClaimsModule.Application.Features.Claims.Commands.RemoveClaimParty;

public class RemoveClaimPartyCommandValidator : AbstractValidator<RemoveClaimPartyCommand>
{
    public RemoveClaimPartyCommandValidator()
    {
        RuleFor(v => v.ClaimId)
            .NotEmpty().WithMessage("Claim ID is required.");

        RuleFor(v => v.ClaimPartyId)
            .NotEmpty().WithMessage("Claim party ID is required.");
    }
}
