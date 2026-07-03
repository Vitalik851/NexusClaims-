using MediatR;
using AutoMapper;
using ClaimsModule.Domain.Entities;
using ClaimsModule.Domain.Interfaces;
using ClaimsModule.Domain.Events;
using ClaimsModule.Application.Common.Models;
using ClaimsModule.Application.Common.Exceptions;
using ClaimsModule.Application.DTOs.Documents;

namespace ClaimsModule.Application.Features.Documents.Commands.UploadDocument;

public class UploadDocumentCommandHandler : IRequestHandler<UploadDocumentCommand, Result<ClaimDocumentDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IStorageService _storageService;
    private readonly IAuditLogService _auditLogService;

    public UploadDocumentCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IStorageService storageService,
        IAuditLogService auditLogService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _storageService = storageService;
        _auditLogService = auditLogService;
    }

    public async Task<Result<ClaimDocumentDto>> Handle(UploadDocumentCommand request, CancellationToken cancellationToken)
    {
        var claim = await _unitOfWork.Claims.GetByIdAsync(request.ClaimId, cancellationToken);
        if (claim == null)
        {
            throw new NotFoundException(nameof(Claim), request.ClaimId);
        }

        // Sanitise filename to prevent path traversal
        var sanitizedFilename = Path.GetFileName(request.DocumentName);
        var containerPath = $"{claim.OrganizationEntityId}/{claim.ClaimId}";

        // Upload to storage
        var blobPath = await _storageService.UploadAsync(
            containerPath, 
            sanitizedFilename, 
            request.FileStream, 
            request.ContentType, 
            cancellationToken);

        // Save metadata
        var document = new ClaimDocument
        {
            ClaimId = claim.ClaimId,
            DocumentType = request.DocumentType,
            DocumentName = sanitizedFilename,
            BlobPath = blobPath,
            ContentType = request.ContentType,
            FileSizeBytes = request.FileSizeBytes,
            UploadedAt = DateTimeOffset.UtcNow,
            UploadedByUserId = request.CurrentUserId,
            Notes = request.Notes
        };

        claim.Documents.Add(document);
        _unitOfWork.Claims.Update(claim);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Raise domain event
        claim.AddDomainEvent(new DocumentUploadedEvent(claim.ClaimId, document.ClaimDocumentId, document.DocumentName));

        // Audit Logging
        await _auditLogService.LogAsync(
            claim.ClaimId,
            "DOCUMENT_UPLOADED",
            $"Uploaded document {document.DocumentName} of type {document.DocumentType}.",
            null,
            null,
            document.ClaimDocumentId,
            "ClaimDocument",
            request.CurrentUserId,
            request.CorrelationId,
            cancellationToken);

        var documentDto = _mapper.Map<ClaimDocumentDto>(document);
        
        // Generate SAS URL with 1-hour TTL
        documentDto.DownloadUrl = await _storageService.GetDownloadUrlAsync(document.BlobPath, TimeSpan.FromHours(1), cancellationToken);

        return Result<ClaimDocumentDto>.Success(documentDto);
    }
}
