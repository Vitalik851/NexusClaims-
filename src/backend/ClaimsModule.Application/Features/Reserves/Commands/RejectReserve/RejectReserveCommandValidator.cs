using FluentValidation;

namespace ClaimsModule.Application.Features.Reserves.Commands.RejectReserve;

public class RejectReserveCommandValidator : AbstractValidator<RejectReserveCommand>
{
    public RejectReserveCommandValidator()
    {
        RuleFor(v => v.ReserveHistoryId)
            .NotEmpty().WithMessage("Reserve history ID is required.");

        RuleFor(v => v.RejectionReason)
            .NotEmpty().WithMessage("Rejection reason is required.")
            .MaximumLength(500).WithMessage("Rejection reason cannot exceed 500 characters.");
    }
}
