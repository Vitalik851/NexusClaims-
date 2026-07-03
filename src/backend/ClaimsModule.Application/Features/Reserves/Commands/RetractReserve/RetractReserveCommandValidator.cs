using FluentValidation;

namespace ClaimsModule.Application.Features.Reserves.Commands.RetractReserve;

public class RetractReserveCommandValidator : AbstractValidator<RetractReserveCommand>
{
    public RetractReserveCommandValidator()
    {
        RuleFor(v => v.ReserveHistoryId)
            .NotEmpty().WithMessage("Reserve history ID is required.");
    }
}
