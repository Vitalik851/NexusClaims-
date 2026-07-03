using AutoMapper;
using ClaimsModule.Domain.Entities;
using ClaimsModule.Application.DTOs.Reserves;

namespace ClaimsModule.Application.Mappings;

public class ReserveProfile : Profile
{
    public ReserveProfile()
    {
        CreateMap<ClaimReserveComponent, ReserveComponentSummaryDto>()
            .ForMember(d => d.Component, opt => opt.MapFrom(s => s.Component.ToString()));

        CreateMap<ReserveHistory, ReserveHistoryDto>()
            .ForMember(d => d.TransactionType, opt => opt.MapFrom(s => s.TransactionType.ToString()))
            .ForMember(d => d.ApprovalStatus, opt => opt.MapFrom(s => s.ApprovalStatus.ToString()))
            .ForMember(d => d.PostingStatus, opt => opt.MapFrom(s => s.PostingStatus.ToString()));
    }
}
