using FluentValidation;
using ClaimsModule.Domain.Enumerations;

namespace ClaimsModule.Application.Features.Claims.Commands.CreateClaim;

public class CreateClaimCommandValidator : AbstractValidator<CreateClaimCommand>
{
    public CreateClaimCommandValidator()
    {
        RuleFor(v => v.LossDate)
            .NotEmpty().WithMessage("Loss date is required.")
            .Must(date => date <= DateTimeOffset.UtcNow.AddHours(24))
            .WithMessage("Loss date cannot be in the future.");

        RuleFor(v => v.LossDescription)
            .NotEmpty().WithMessage("Loss description is required.")
            .MinimumLength(20).WithMessage("Loss description is required and must be at least 20 characters.");

        RuleFor(v => v.CauseOfLossCode)
            .NotEmpty().WithMessage("Cause of loss code is required.");

        RuleFor(v => v.OrganizationEntityId)
            .NotEmpty().WithMessage("OrganizationEntityId is required.");

        RuleFor(v => v.InitialReserve)
            .ChildRules(reserve =>
            {
                reserve.RuleFor(r => r.Component)
                    .NotEmpty().WithMessage("Reserve component is required.")
                    .IsEnumName(typeof(ReserveComponentType), false).WithMessage("Invalid reserve component type.");

                reserve.RuleFor(r => r.Amount)
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

                reserve.RuleFor(r => r.ChangeReason)
                    .NotEmpty().WithMessage("Change reason is required.")
                    .MaximumLength(500).WithMessage("Change reason cannot exceed 500 characters.");
            })
            .When(v => v.InitialReserve != null);

        RuleFor(v => v.Parties)
            .Must(parties => parties != null && parties.Any(p => string.Equals(p.PartyRole, "Claimant", StringComparison.OrdinalIgnoreCase)))
            .WithMessage("A claim must have at least one Claimant party (BR-C-03).");

        RuleForEach(v => v.Parties)
            .ChildRules(party =>
            {
                party.RuleFor(p => p.PartyRole)
                    .NotEmpty().WithMessage("Party role is required.")
                    .IsEnumName(typeof(PartyRole), false).WithMessage("Invalid party role.");

                party.RuleFor(p => p.PartyType)
                    .NotEmpty().WithMessage("Party type is required.")
                    .IsEnumName(typeof(PartyType), false).WithMessage("Invalid party type.");

                party.RuleFor(p => p.Email)
                    .EmailAddress().WithMessage("A valid email address is required.")
                    .When(p => !string.IsNullOrEmpty(p.Email));
            });
    }
}
