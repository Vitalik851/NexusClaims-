using AutoMapper;
using ClaimsModule.Domain.Entities;
using ClaimsModule.Application.DTOs.Claims;
using ClaimsModule.Application.DTOs.Audit;

namespace ClaimsModule.Application.Mappings;

public class ClaimProfile : Profile
{
    public ClaimProfile()
    {
        CreateMap<Claim, ClaimSummaryDto>()
            .ForMember(d => d.ClaimId, opt => opt.MapFrom(s => s.ClaimId))
            .ForMember(d => d.LossDate, opt => opt.MapFrom(s => s.LossEvent.LossDate))
            .ForMember(d => d.CauseOfLossCode, opt => opt.MapFrom(s => s.LossEvent.CauseOfLossCode))
            .ForMember(d => d.CauseOfLossName, opt => opt.MapFrom(s => s.LossEvent.CauseOfLossCodeNavigation != null ? s.LossEvent.CauseOfLossCodeNavigation.Name : s.LossEvent.CauseOfLossCode))
            .ForMember(d => d.TotalReserves, opt => opt.MapFrom(s => s.ReserveComponents
                .Where(rc => rc.Status == "Active")
                .Sum(rc => rc.CurrentAmount)))
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()));

        CreateMap<Claim, ClaimDetailDto>()
            .ForMember(d => d.ClaimId, opt => opt.MapFrom(s => s.ClaimId))
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.Severity, opt => opt.MapFrom(s => s.Severity.HasValue ? s.Severity.Value.ToString() : null))
            .ForMember(d => d.RecentAuditEntries, opt => opt.MapFrom(s => s.AuditLogs.OrderByDescending(a => a.CreatedAt).Take(20)));

        CreateMap<LossEvent, LossEventDto>()
            .ForMember(d => d.CauseOfLossName, opt => opt.MapFrom(s => s.CauseOfLossCodeNavigation != null ? s.CauseOfLossCodeNavigation.Name : s.CauseOfLossCode));

        CreateMap<ClaimParty, ClaimPartyDto>()
            .ForMember(d => d.PartyRole, opt => opt.MapFrom(s => s.PartyRole.ToString()))
            .ForMember(d => d.PartyType, opt => opt.MapFrom(s => s.PartyType.ToString()));

        CreateMap<ClaimRiskObject, ClaimRiskObjectDto>()
            .ForMember(d => d.AssetType, opt => opt.MapFrom(s => s.AssetType.ToString()));

        CreateMap<ClaimAuditLog, AuditLogEntryDto>();
    }
}
// Add extension mapper if needed for Id property mapping
public static class ProfileExtensions
{
    public static IMappingExpression<TSource, TDestination> ForMemberId<TSource, TDestination>(
        this IMappingExpression<TSource, TDestination> map, 
        System.Linq.Expressions.Expression<Func<TDestination, object>> destinationMember, 
        System.Linq.Expressions.Expression<Func<TSource, object>> sourceMember)
    {
        return map.ForMember(destinationMember, opt => opt.MapFrom(sourceMember));
    }
}
