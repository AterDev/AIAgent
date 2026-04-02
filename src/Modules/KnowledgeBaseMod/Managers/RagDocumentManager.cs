using KnowledgeBaseMod.Models.RagDocumentDtos;
using Perigon.AspNetCore.Constants;

namespace KnowledgeBaseMod.Managers;

/// <summary>
/// 文档管理
/// </summary>
public class RagDocumentManager(
    TenantDbFactory dbContextFactory,
    ILogger<RagDocumentManager> logger,
    IUserContext userContext,
    IStorageProviderQuery storageProviderQuery
) : ManagerBase<DefaultDbContext, RagDocument>(dbContextFactory, userContext, logger)
{
    private readonly IStorageProviderQuery _storageProviderQuery = storageProviderQuery;

    /// <summary>
    /// 支持的文档类型（文件扩展名，不含点）
    /// </summary>
    private static readonly HashSet<string> SupportedFileTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        // 文本和文档格式
        "txt", "md", "json", "xml", "csv", "log",
        // Office 和 PDF
        "pdf", "docx", "xlsx", "xls", "pptx",
        // 图片格式（OCR 支持）
        "jpg", "jpeg", "png"
    };
    public async Task<PageList<RagDocumentItemDto>> FilterAsync(RagDocumentFilterDto filter)
    {
        Queryable = BuildScopedQuery(filter.ApplicationId)
            .WhereNotNull(filter.CollectionId, q => q.CollectionId == filter.CollectionId)
            .WhereNotNull(filter.Name, q => q.Name == filter.Name)
            .WhereNotNull(filter.Status, q => q.Status == filter.Status);

        return await PageListAsync<RagDocumentFilterDto, RagDocumentItemDto>(filter);
    }

    public async Task<RagDocument> AddAsync(RagDocumentAddDto dto)
    {
        var collection = await GetRequiredAccessibleCollectionAsync(dto.CollectionId, dto.ApplicationId);

        var entity = dto.MapTo<RagDocument>();
        entity.CollectionId = collection.Id;
        
        // 自动设置活跃的存储服务商
        var activeProvider = await GetRequiredActiveStorageProviderAsync();
        entity.StorageProviderId = activeProvider.Id;
        
        // 根据文件后缀自动设置FileType及其他元数据
        entity.FileType = ValidateAndGetFileType(entity.FileName);
        
        await InsertAsync(entity);
        return entity;
    }

    public async Task EnsureAddRequestValidAsync(RagDocumentAddDto dto)
    {
        _ = await GetRequiredAccessibleCollectionAsync(dto.CollectionId, dto.ApplicationId);
        _ = await GetRequiredActiveStorageProviderAsync();
        _ = ValidateAndGetFileType(dto.FileName);
    }

    public async Task<int> EditAsync(Guid id, RagDocumentUpdateDto dto)
    {
        if (!await HasPermissionAsync(id))
        {
            throw new BusinessException(Localizer.NoPermission, StatusCodes.Status403Forbidden);
        }

        return await UpdateAsync(id, dto);
    }

    public async Task<RagDocumentDetailDto?> GetAsync(Guid id)
    {
        return await BuildScopedQuery()
            .Where(q => q.Id == id)
            .Select(q => new RagDocumentDetailDto
            {
                Id = q.Id,
                CollectionId = q.CollectionId,
                CreatedTime = q.CreatedTime,
                UpdatedTime = q.UpdatedTime,
                TenantId = q.TenantId,
                Name = q.Name,
                FileName = q.FileName,
                FilePath = q.FilePath,
                FileType = q.FileType,
                StorageProviderId = q.StorageProviderId,
                Status = q.Status,
                Tags = q.Tags,
                Roles = q.Roles,
                ChunkCount = q.ChunkCount,
                TokenCount = q.TokenCount,
                ErrorMessage = q.ErrorMessage,
            })
            .FirstOrDefaultAsync();
    }

    public async Task<bool?> DeleteAsync(List<Guid> ids, bool softDelete = true)
    {
        if (!ids.Any())
        {
            return false;
        }

        var ownedIds = await BuildScopedQuery()
            .Where(q => ids.Contains(q.Id))
            .Select(q => q.Id)
            .ToListAsync();

        if (!ownedIds.Any())
        {
            throw new BusinessException(Localizer.NoPermission, StatusCodes.Status403Forbidden);
        }

        return await DeleteOrUpdateAsync(ownedIds, !softDelete) > 0;
    }

    public async Task<bool> QueueIngestionAsync(Guid id)
    {
        var document = await BuildScopedQuery()
            .FirstOrDefaultAsync(q => q.Id == id);

        if (document == null)
        {
            throw new BusinessException(Localizer.NoPermission, StatusCodes.Status403Forbidden);
        }

        document.Status = RagDocumentStatus.Pending;
        document.ErrorMessage = null;
        document.RetryCount = 0;

        await _dbContext.SaveChangesAsync();
        return true;
    }

    public override async Task<bool> HasPermissionAsync(Guid id)
    {
        return await BuildScopedQuery().AnyAsync(q => q.Id == id);
    }

    private IQueryable<RagDocument> BuildScopedQuery(Guid? requestedApplicationId = null)
    {
        var query = _dbSet
            .Include(q => q.Collection)
            .Where(q => q.TenantId == _userContext.TenantId);

        if (_userContext.IsRole(WebConst.Application))
        {
            return query.Where(q => _dbContext.ApplicationRagCollectionPermissions
                .Any(link => link.TenantId == _userContext.TenantId
                    && link.IsEnabled
                    && link.ApplicationId == _userContext.UserId
                    && link.RagCollectionId == q.CollectionId));
        }

        var applicationId = requestedApplicationId;
        if (applicationId.HasValue && applicationId != Guid.Empty)
        {
            return query.Where(q => _dbContext.ApplicationRagCollectionPermissions
                .Any(link => link.TenantId == _userContext.TenantId
                    && link.IsEnabled
                    && link.ApplicationId == applicationId
                    && link.RagCollectionId == q.CollectionId));
        }

        return query;
    }

    private async Task<RagCollection> GetRequiredAccessibleCollectionAsync(Guid collectionId, Guid? requestedApplicationId)
    {
        var query = _dbContext.RagCollections.Where(q => q.Id == collectionId && q.TenantId == _userContext.TenantId);

        if (_userContext.IsRole(WebConst.Application))
        {
            query = query.Where(q => _dbContext.ApplicationRagCollectionPermissions
                .Any(link => link.TenantId == _userContext.TenantId
                    && link.IsEnabled
                    && link.ApplicationId == _userContext.UserId
                    && link.RagCollectionId == q.Id));
        }

        if (requestedApplicationId.HasValue && requestedApplicationId != Guid.Empty)
        {
            query = query.Where(q => _dbContext.ApplicationRagCollectionPermissions
                .Any(link => link.TenantId == _userContext.TenantId
                    && link.IsEnabled
                    && link.ApplicationId == requestedApplicationId
                    && link.RagCollectionId == q.Id));
        }

        var collection = await query.FirstOrDefaultAsync();
        if (collection == null)
        {
            throw new BusinessException(Localizer.NoPermission, StatusCodes.Status403Forbidden);
        }

        return collection;
    }

    private async Task<StorageProviderInfo> GetRequiredActiveStorageProviderAsync()
    {
        var activeProvider = await _storageProviderQuery.GetActiveProviderAsync();
        if (activeProvider == null)
        {
            throw new BusinessException(Localizer.NoActiveStorageProviderConfigured);
        }

        return activeProvider;
    }

    private static string ValidateAndGetFileType(string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new BusinessException("FileName is required for document creation");
        }

        var extension = Path.GetExtension(fileName).TrimStart('.').ToLowerInvariant();
        if (!SupportedFileTypes.Contains(extension))
        {
            throw new BusinessException(
                $"Unsupported file type: {extension}. " +
                $"Supported formats: {string.Join(", ", SupportedFileTypes.OrderBy(x => x))}");
        }

        return extension;
    }
}
