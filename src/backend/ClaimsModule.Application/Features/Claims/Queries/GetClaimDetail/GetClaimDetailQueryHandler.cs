using MediatR;
using AutoMapper;
using ClaimsModule.Domain.Entities;
using ClaimsModule.Domain.Interfaces;
using ClaimsModule.Application.Common.Exceptions;
using ClaimsModule.Application.DTOs.Claims;

namespace ClaimsModule.Application.Features.Claims.Queries.GetClaimDetail;

public class GetClaimDetailQueryHandler : IRequestHandler<GetClaimDetailQuery, ClaimDetailDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IStorageService _storageService;

    public GetClaimDetailQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, IStorageService storageService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _storageService = storageService;
    }

    public async Task<ClaimDetailDto> Handle(GetClaimDetailQuery request, CancellationToken cancellationToken)
    {
        var claim = await _unitOfWork.Claims.GetByIdWithDetailsAsync(request.ClaimId, cancellationToken);
        if (claim == null)
        {
            throw new NotFoundException(nameof(Claim), request.ClaimId);
        }

        var dto = _mapper.Map<ClaimDetailDto>(claim);

        // Generate download URLs for each document (1-hour expiry)
        foreach (var docDto in dto.Documents)
        {
            var match = claim.Documents.First(d => d.ClaimDocumentId == docDto.ClaimDocumentId);
            docDto.DownloadUrl = await _storageService.GetDownloadUrlAsync(match.BlobPath, TimeSpan.FromHours(1), cancellationToken);
        }

        return dto;
    }
}
