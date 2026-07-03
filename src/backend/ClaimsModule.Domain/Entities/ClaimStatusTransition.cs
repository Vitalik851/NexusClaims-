using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Enumerations;

namespace ClaimsModule.Domain.Entities;

/// <summary>
/// Defines a valid state-machine transition between two <see cref="ClaimStatus"/> values.
/// Optionally requires a named permission for authorisation.
/// </summary>
public class ClaimStatusTransition : BaseEntity
{
    public ClaimStatus FromStatus { get; set; }

    public ClaimStatus ToStatus { get; set; }

    /// <summary>
    /// Optional permission string the user must possess to perform this transition.
    /// Null means anyone with claim access may perform the transition.
    /// </summary>
    public string? RequiredPermission { get; set; }
}
