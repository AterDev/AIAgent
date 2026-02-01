using Entity.KnowledgeBaseMod;

namespace KnowledgeBaseMod.Managers;

public class DocumentParsingResultManager(
    TenantDbFactory dbContextFactory,
    ILogger<DocumentParsingResultManager> logger,
    IUserContext userContext
) : ManagerBase<DefaultDbContext, DocumentParsingResult>(dbContextFactory, userContext, logger)
{
    public async Task<DocumentParsingResult> CreateAsync(Guid ragDocumentId, DocumentFormatType format, string content)
    {
        var entity = new DocumentParsingResult
        {
            RagDocumentId = ragDocumentId,
            DocumentFormat = format,
            ParsingStatus = DocumentParsingStatus.Success,
            TextContent = content,
            WordCount = content.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length,
            TenantId = _userContext.TenantId,
            CompletedTime = DateTime.UtcNow,
            DurationMs = 0,
            ParsingVersion = 1,
            RagDocument = null!
        };

        await InsertAsync(entity);
        return entity;
    }

    public async Task<DocumentParsingResult?> GetLatestAsync(Guid ragDocumentId)
    {
        return await _dbSet
            .Where(q => q.RagDocumentId == ragDocumentId && q.TenantId == _userContext.TenantId)
            .OrderByDescending(q => q.CreatedTime)
            .FirstOrDefaultAsync();
    }

    public async Task<List<DocumentParsingResult>> GetByDocumentAsync(Guid ragDocumentId)
    {
        return await _dbSet
            .Where(q => q.RagDocumentId == ragDocumentId && q.TenantId == _userContext.TenantId)
            .OrderByDescending(q => q.CreatedTime)
            .ToListAsync();
    }

    public override async Task<bool> HasPermissionAsync(Guid id)
    {
        return await _dbSet.AnyAsync(q => q.Id == id && q.TenantId == _userContext.TenantId);
    }
}
