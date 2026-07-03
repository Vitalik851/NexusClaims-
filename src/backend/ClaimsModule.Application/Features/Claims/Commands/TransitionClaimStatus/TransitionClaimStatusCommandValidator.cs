using FluentValidation;
using ClaimsModule.Domain.Enumerations;

namespace ClaimsModule.Application.Features.Claims.Commands.TransitionClaimStatus;

public class TransitionClaimStatusCommandValidator : AbstractValidator<TransitionClaimStatusCommand>
{
    public TransitionClaimStatusCommandValidator()
    {
        RuleFor(v => v.ClaimId)
            .NotEmpty().WithMessage("Claim ID is required.");

        RuleFor(v => v.TargetStatus)
            .NotEmpty().WithMessage("Target status is required.")
            .IsEnumName(typeof(ClaimStatus), false).WithMessage("Invalid target status.");
    }
}
