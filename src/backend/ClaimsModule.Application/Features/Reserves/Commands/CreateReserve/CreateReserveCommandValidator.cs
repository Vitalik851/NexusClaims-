using FluentValidation;
using ClaimsModule.Domain.Enumerations;

namespace ClaimsModule.Application.Features.Reserves.Commands.CreateReserve;

public class CreateReserveCommandValidator : AbstractValidator<CreateReserveCommand>
{
    public CreateReserveCommandValidator()
    {
        RuleFor(v => v.ClaimId)
            .NotEmpty().WithMessage("Claim ID is required.");

        RuleFor(v => v.Component)
            .NotEmpty().WithMessage("Component is required.")
            .IsEnumName(typeof(ReserveComponentType), false).WithMessage("Invalid reserve component type.");

        RuleFor(v => v.TransactionType)
            .NotEmpty().WithMessage("Transaction type is required.")
            .IsEnumName(typeof(TransactionType), false).WithMessage("Invalid transaction type.");

        RuleFor(v => v.ChangeReason)
            .NotEmpty().WithMessage("Change reason is required.")
            .MaximumLength(500).WithMessage("Change reason cannot exceed 500 characters.");

        RuleFor(v => v.Amount)
            .Must((req, amount) =>
            {
                if (Enum.TryParse<ReserveComponentType>(req.Component, out var compType))
                {
                    if (compType == ReserveComponentType.SubrogationRecoverable)
                    {
                        return true; // Can go negative
                    }
                }
                return amount > 0;
            })
            .WithMessage("Reserve amount must be greater than zero.");
    }
}
