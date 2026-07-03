using MediatR;
using AutoMapper;
using ClaimsModule.Domain.Entities;
using ClaimsModule.Domain.Interfaces;
using ClaimsModule.Application.Common.Exceptions;
using ClaimsModule.Application.DTOs.Documents;

namespace ClaimsModule.Application.Features.Documents.Queries.ListDocuments;

public class ListDocumentsQueryHandler : IRequestHandler<ListDocumentsQuery, IEnumerable<ClaimDocumentDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IStorageService _storageService;

    public ListDocumentsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, IStorageService storageService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _storageService = storageService;
    }

    public async Task<IEnumerable<ClaimDocumentDto>> Handle(ListDocumentsQuery request, CancellationToken cancellationToken)
    {
        var claim = await _unitOfWork.Claims.GetByIdWithDetailsAsync(request.ClaimId, cancellationToken);
        if (claim == null)
        {
            throw new NotFoundException(nameof(Claim), request.ClaimId);
        }

        var dtos = _mapper.Map<List<ClaimDocumentDto>>(claim.Documents);

        foreach (var docDto in dtos)
        {
            var match = claim.Documents.First(d => d.ClaimDocumentId == docDto.ClaimDocumentId);
            docDto.DownloadUrl = await _storageService.GetDownloadUrlAsync(match.BlobPath, TimeSpan.FromHours(1), cancellationToken);
        }

        return dtos;
    }
}
