using KnowledgeBaseMod.Models.RagDocumentDtos;
using Share.Services;

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
        Queryable = Queryable
            .Where(q => q.TenantId == _userContext.TenantId)
            .WhereNotNull(filter.CollectionId, q => q.CollectionId == filter.CollectionId)
            .WhereNotNull(filter.Name, q => q.Name == filter.Name)
            .WhereNotNull(filter.Status, q => q.Status == filter.Status);

        return await PageListAsync<RagDocumentFilterDto, RagDocumentItemDto>(filter);
    }

    public async Task<RagDocument> AddAsync(RagDocumentAddDto dto)
    {
        var entity = dto.MapTo<RagDocument>();
        
        // 自动设置活跃的存储服务商
        var activeProvider = await _storageProviderQuery.GetActiveProviderAsync();
        if (activeProvider == null)
        {
            throw new BusinessException(Localizer.NoActiveStorageProviderConfigured);
        }
        entity.StorageProviderId = activeProvider.Id;
        
        // 根据文件后缀自动设置FileType及其他元数据
        if (!string.IsNullOrEmpty(entity.FileName))
        {
            var extension = Path.GetExtension(entity.FileName).TrimStart('.').ToLower();
            
            // 验证文件类型是否支持
            if (!SupportedFileTypes.Contains(extension))
            {
                throw new BusinessException(
                    $"Unsupported file type: {extension}. " +
                    $"Supported formats: {string.Join(", ", SupportedFileTypes.OrderBy(x => x))}");
            }
            
            entity.FileType = extension;
        }
        else
        {
            throw new BusinessException("FileName is required for document creation");
        }
        
        await InsertAsync(entity);
        return entity;
    }

    public async Task<int> EditAsync(Guid id, RagDocumentUpdateDto dto)
    {
        return await UpdateAsync(id, dto);
    }

    public async Task<RagDocumentDetailDto?> GetAsync(Guid id)
    {
        return await FindAsync<RagDocumentDetailDto>(q => q.Id == id && q.TenantId == _userContext.TenantId);
    }

    public async Task<bool?> DeleteAsync(List<Guid> ids, bool softDelete = true)
    {
        if (!ids.Any())
        {
            return false;
        }
        return await DeleteOrUpdateAsync(ids, !softDelete) > 0;
    }

    public override async Task<bool> HasPermissionAsync(Guid id)
    {
        return await _dbSet.AnyAsync(q => q.Id == id && q.TenantId == _userContext.TenantId);
    }
}
