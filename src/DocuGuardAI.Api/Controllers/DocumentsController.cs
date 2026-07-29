using System.Security.Claims;
using DocuGuardAI.Api.Extensions;
using DocuGuardAI.Application.Features.Documents.Commands.Delete;
using DocuGuardAI.Application.Features.Documents.Commands.Upload;
using DocuGuardAI.Application.Features.Documents.Queries.GetUserDocuments;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocuGuardAI.Api.Controllers;

[ApiController]
[Route("documents")]
[Authorize]
[IgnoreAntiforgeryToken]
public class DocumentsController(IMediator mediator, IWebHostEnvironment environment) : ControllerBase
{
    private static readonly long MaxFileSize = 100 * 1024 * 1024;
    private readonly string _uploadPath = Path.Combine(environment.ContentRootPath, "uploads");

    [HttpPost("upload")]
    [Authorize(Policy = "EditorOrAdmin")]
    public async Task<IActionResult> UploadDocument([FromForm] IFormFile file)
    {
        if (file.Length == 0)
            return BadRequest("File is required");

        if (file.Length > MaxFileSize)
            return BadRequest($"File size must not exceed {MaxFileSize / 1024 / 1024} MB");

        if (!IsValidContentType(file.ContentType))
            return BadRequest("Invalid file type");

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            return Unauthorized();

        Directory.CreateDirectory(_uploadPath);
        var fileName = $"{Guid.NewGuid()}_{file.FileName}";
        var filePath = Path.Combine(_uploadPath, fileName);

        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var command = new UploadDocumentCommand(
            userGuid,
            file.FileName,
            filePath,
            file.ContentType,
            file.Length);

        var result = await mediator.Send(command);
        return result.ToActionResult(this);
    }

    [HttpGet]
    [Authorize(Policy = "AnyRole")]
    public async Task<IActionResult> GetUserDocuments()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            return Unauthorized();

        var query = new GetUserDocumentsQuery(userGuid);
        var result = await mediator.Send(query);

        return result.ToActionResult(this);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "EditorOrAdmin")]
    public async Task<IActionResult> DeleteDocument(Guid id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            return Unauthorized();

        var command = new DeleteDocumentCommand(id, userGuid);
        var result = await mediator.Send(command);

        return result.ToActionResult(this);
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