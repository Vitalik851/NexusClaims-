using AutoMapper;
using ClaimsModule.Domain.Entities;
using ClaimsModule.Application.DTOs.Policies;

namespace ClaimsModule.Application.Mappings;

public class PolicyProfile : Profile
{
    public PolicyProfile()
    {
        CreateMap<Policy, PolicySearchResultDto>()
            .ForMember(d => d.CoverageTypes, opt => opt.MapFrom(s => (s.CoverageTypes ?? string.Empty)
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Trim())));
    }
}
