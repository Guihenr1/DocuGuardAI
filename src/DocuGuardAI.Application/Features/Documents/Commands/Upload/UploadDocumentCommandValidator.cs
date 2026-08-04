using DocuGuardAI.Application.Common.Constants;
using FluentValidation;

namespace DocuGuardAI.Application.Features.Documents.Commands.Upload;

public class UploadDocumentCommandValidator : AbstractValidator<UploadDocumentCommand>
{
    public UploadDocumentCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.FileName)
            .NotEmpty()
            .MaximumLength(255)
            .Must(DocumentFileExtensions.IsAllowed)
            .WithMessage(x =>
                $"File extension is not supported. Allowed extensions: " +
                string.Join(", ", DocumentFileExtensions.Allowed));

        RuleFor(x => x.FileSize)
            .GreaterThan(0).WithMessage("File size must be greater than 0")
            .LessThanOrEqualTo(100 * 1024 * 1024).WithMessage("File size must not exceed 100 MB");

        RuleFor(x => x.ContentType)
            .NotEmpty().WithMessage("Content type is required")
            .Must(IsValidContentType).WithMessage("Invalid content type");
    }

    private static bool IsValidContentType(string contentType)
    {
        var allowedTypes = new[]
        {
            "application/pdf",
            "text/plain",
            "application/msword",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "image/jpeg",
            "image/png"
        };

        return allowedTypes.Contains(contentType);
    }
}