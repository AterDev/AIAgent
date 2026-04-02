using CoreMod.Services;
using Entity.KnowledgeBaseMod;
using KnowledgeBaseMod.Managers;
using KnowledgeBaseMod.Models.RagDocumentDtos;
using Share.Abstraction;
using Share.Models;

namespace ApiService.Controllers.OpenPlatform;

/// <summary>
/// Open platform rag documents
/// </summary>
[ApiController]
[Route("api/v1/rag/documents")]
public class RagDocumentsController(
    RagDocumentManager manager,
    IUserContext user,
    ILogger<RagDocumentsController> logger,
    IFileStorageService fileStorageService,
    NatsRagMessagePublisher messagePublisher
) : OpenApiControllerBase<RagDocumentManager>(manager, user, logger)
{
    private readonly IFileStorageService _fileStorageService = fileStorageService;
    private readonly NatsRagMessagePublisher _messagePublisher = messagePublisher;

    [HttpPost("filter")]
    public async Task<ActionResult<PageList<RagDocumentItemDto>>> ListAsync(RagDocumentFilterDto filter)
    {
        return await _manager.FilterAsync(filter);
    }

    [HttpPost]
    public async Task<ActionResult<RagDocument>> AddAsync(RagDocumentAddDto dto)
    {
        if (_user.IsRole(WebConst.Application))
        {
            dto.ApplicationId = _user.UserId;
        }

        var entity = await _manager.AddAsync(dto);
        return CreatedAtRoute(null, new { id = entity.Id }, entity);
    }

    [HttpGet("{id}")]
    public async Task<RagDocumentDetailDto?> DetailAsync([FromRoute] Guid id)
    {
        return await _manager.GetAsync(id);
    }

    [HttpPatch("{id}")]
    public async Task<bool> UpdateAsync([FromRoute] Guid id, RagDocumentUpdateDto dto)
    {
        return await _manager.EditAsync(id, dto) == 1;
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<bool>> DeleteAsync([FromRoute] Guid id)
    {
        return await _manager.DeleteAsync([id], false);
    }

    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<RagDocument>> UploadAsync([FromForm] RagDocumentUploadRequestDto request, CancellationToken cancellationToken)
    {
        var normalizedName = string.IsNullOrWhiteSpace(request.Name)
            ? Path.GetFileNameWithoutExtension(request.File.FileName)
            : request.Name.Trim();

        await _manager.EnsureAddRequestValidAsync(new RagDocumentAddDto
        {
            ApplicationId = _user.IsRole(WebConst.Application) ? _user.UserId : null,
            CollectionId = request.CollectionId,
            Name = normalizedName,
            FileName = request.File.FileName,
            Tags = request.Tags,
            Roles = request.Roles,
            Status = RagDocumentStatus.Pending,
        });

        FileUploadResult? uploadResult = null;

        try
        {
            using var stream = request.File.OpenReadStream();
            uploadResult = await _fileStorageService.UploadAsync(stream, request.File.FileName, "document", cancellationToken);

            var document = await _manager.AddAsync(new RagDocumentAddDto
            {
                ApplicationId = _user.IsRole(WebConst.Application) ? _user.UserId : null,
                CollectionId = request.CollectionId,
                Name = normalizedName,
                FileName = request.File.FileName,
                FilePath = uploadResult.FilePath,
                Status = RagDocumentStatus.Pending,
                Tags = request.Tags,
                Roles = request.Roles,
            });

            if (request.AutoParse)
            {
                var detail = await _manager.GetAsync(document.Id);
                if (detail != null)
                {
                    await PublishParseAsync(detail, cancellationToken);
                }
            }

            return CreatedAtRoute(null, new { id = document.Id }, document);
        }
        catch
        {
            if (uploadResult != null)
            {
                var deleted = await _fileStorageService.DeleteAsync(uploadResult.FilePath, uploadResult.IsCloud, cancellationToken);
                if (!deleted)
                {
                    _logger.LogWarning("Failed to cleanup uploaded file after document creation failure: {FilePath}", uploadResult.FilePath);
                }
            }

            throw;
        }
    }

    [HttpPost("{id}/parse")]
    public async Task<ActionResult> TriggerParseAsync([FromRoute] Guid id, CancellationToken cancellationToken = default)
    {
        var document = await _manager.GetAsync(id);
        if (document == null)
        {
            return NotFound();
        }

        await PublishParseAsync(document, cancellationToken);
        return Ok();
    }

    private async Task PublishParseAsync(RagDocumentDetailDto document, CancellationToken cancellationToken)
    {
        await _messagePublisher.PublishAsync(new RagIngestionMessage
        {
            DocumentId = document.Id,
            TenantId = _user.TenantId,
            CollectionId = document.CollectionId,
            FilePath = document.FilePath ?? string.Empty,
            FileType = document.FileType ?? "txt",
            DocumentName = document.Name ?? string.Empty,
            FileName = document.FileName ?? string.Empty,
            StorageProviderId = document.StorageProviderId,
        }, cancellationToken);
    }
}