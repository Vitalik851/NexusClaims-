using AutoMapper;
using ClaimsModule.Domain.Entities;
using ClaimsModule.Application.DTOs.Documents;

namespace ClaimsModule.Application.Mappings;

public class DocumentProfile : Profile
{
    public DocumentProfile()
    {
        CreateMap<ClaimDocument, ClaimDocumentDto>()
            .ForMember(d => d.DownloadUrl, opt => opt.Ignore()); // Will be populated in handlers using IStorageService
    }
}
