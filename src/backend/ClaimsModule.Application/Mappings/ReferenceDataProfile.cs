using AutoMapper;
using ClaimsModule.Domain.Entities;
using ClaimsModule.Application.DTOs.Reference;

namespace ClaimsModule.Application.Mappings;

public class ReferenceDataProfile : Profile
{
    public ReferenceDataProfile()
    {
        CreateMap<CauseOfLossCode, CauseOfLossCodeDto>();
        CreateMap<ClaimStatusTransition, ClaimStatusTransitionDto>()
            .ForMember(d => d.FromStatus, opt => opt.MapFrom(s => s.FromStatus.ToString()))
            .ForMember(d => d.ToStatus, opt => opt.MapFrom(s => s.ToStatus.ToString()));
    }
}
