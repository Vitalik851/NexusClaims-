using FluentValidation;

namespace ClaimsModule.Application.Features.Reserves.Commands.ApproveReserve;

public class ApproveReserveCommandValidator : AbstractValidator<ApproveReserveCommand>
{
    public ApproveReserveCommandValidator()
    {
        RuleFor(v => v.ReserveHistoryId)
            .NotEmpty().WithMessage("Reserve history ID is required.");
    }
}
