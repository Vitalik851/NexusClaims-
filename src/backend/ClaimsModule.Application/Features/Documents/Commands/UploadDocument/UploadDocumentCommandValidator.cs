using FluentValidation;

namespace ClaimsModule.Application.Features.Documents.Commands.UploadDocument;

public class UploadDocumentCommandValidator : AbstractValidator<UploadDocumentCommand>
{
    private static readonly string[] AllowedMimeTypes = 
    [
        "application/pdf",
        "image/jpeg",
        "image/png",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document", // docx
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", // xlsx
        "text/plain",
        "text/csv"
    ];

    public UploadDocumentCommandValidator()
    {
        RuleFor(v => v.ClaimId)
            .NotEmpty().WithMessage("Claim ID is required.");

        RuleFor(v => v.DocumentName)
            .NotEmpty().WithMessage("Document name is required.")
            .MaximumLength(255).WithMessage("Document name cannot exceed 255 characters.");

        RuleFor(v => v.DocumentType)
            .NotEmpty().WithMessage("Document type is required.");

        RuleFor(v => v.ContentType)
            .NotEmpty().WithMessage("Content type is required.")
            .Must(contentType => AllowedMimeTypes.Contains(contentType.ToLower()))
            .WithMessage("Unsupported file format. Supported formats: PDF, JPEG, PNG, DOCX, XLSX, TXT, CSV.");

        RuleFor(v => v.FileSizeBytes)
            .LessThanOrEqualTo(50 * 1024 * 1024)
            .WithMessage("File size exceeds the maximum limit of 50 MB.");

        RuleFor(v => v.FileStream)
            .NotNull().WithMessage("File stream is required.");
    }
}
