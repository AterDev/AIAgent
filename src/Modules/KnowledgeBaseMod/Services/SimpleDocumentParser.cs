using Perigon.AspNetCore.Toolkit.Services;
using SystemMod.Managers;
using SystemMod.Services;

namespace KnowledgeBaseMod.Services;

/// <summary>
/// 简单文档解析（仅文本/markdown）
/// </summary>
public class SimpleDocumentParser(
    IHttpClientFactory httpClientFactory,
    IFileStorageService fileStorageService,
    StorageProviderManager storageProviderManager,
    ILogger<SimpleDocumentParser> logger
) : IDocumentParser
{
    public async Task<DocumentParseResult> ParseAsync(RagDocument document, string? rawContent, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(rawContent))
        {
            return ToResult(rawContent, document.FileType);
        }

        // 优先处理文件路径
        if (!string.IsNullOrWhiteSpace(document.FilePath) && document.StorageProviderId != Guid.Empty)
        {
            // 先查询提供商信息，避免重复查询
            var provider = await storageProviderManager.FindAsync(document.StorageProviderId);
            if (provider == null)
            {
                logger.LogWarning("Storage provider not found: {StorageProviderId}", document.StorageProviderId);
                throw new BusinessException($"Storage provider not found: {document.StorageProviderId}");
            }

            var localPath = await fileStorageService.DownloadFileAsync(
                document.StorageProviderId,
                document.FilePath,
                cancellationToken
            );

            if (localPath != null)
            {
                try
                {
                    // 判断文件类型，对图片使用 OCR
                    var fileType = document.FileType.ToLower();
                    if (fileType is "jpeg" or "jpg" or "png")
                    {
                        // 图片格式：OCR 支持尚未实现
                        throw new BusinessException(
                            $"Image documents ({fileType.ToUpper()}) are not yet supported. " +
                            "Please upload documents in PDF or text format, or wait for OCR support in a future release.");
                    }

                    // 文本格式：直接读取
                    var text = await File.ReadAllTextAsync(localPath, cancellationToken);
                    return ToResult(text, document.FileType);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to read file {FilePath}", localPath);
                    throw;
                }
                finally
                {
                    // 清理临时文件（仅限云存储下载的文件）
                    if (provider.IsCloud)
                    {
                        try
                        {
                            fileStorageService.CleanupTempFile(localPath);
                        }
                        catch (Exception ex)
                        {
                            logger.LogWarning(ex, "Failed to cleanup temp file {TempPath}", localPath);
                        }
                    }
                }
            }
            else
            {
                logger.LogWarning("File not found: {FilePath}", document.FilePath);
                throw new BusinessException($"File not found: {document.FilePath}");
            }
        }

        // 如果没有文件路径，尝试从 URL 获取
        if (!string.IsNullOrWhiteSpace(document.SourceUrl) && Uri.TryCreate(document.SourceUrl, UriKind.Absolute, out var uri))
        {
            if (uri.Scheme is not ("https" or "http"))
            {
                throw new BusinessException("SourceUrl must be http/https");
            }

            var client = httpClientFactory.CreateClient();
            var text = await client.GetStringAsync(uri, cancellationToken);
            return ToResult(text, document.FileType);
        }

        logger.LogWarning("No document content available for {DocumentId}", document.Id);
        throw new BusinessException("Document content is empty");
    }

    private static DocumentParseResult ToResult(string text, string? contentType)
    {
        return new DocumentParseResult
        {
            Text = text,
            TokenCount = EstimateTokens(text),
            ContentType = contentType ?? "text/plain",
        };
    }

    private static int EstimateTokens(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return 0;
        }

        return Math.Max(1, text.Length / 4);
    }
}
