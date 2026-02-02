using Perigon.AspNetCore.Toolkit.Services;

namespace KnowledgeBaseMod.Services;

/// <summary>
/// 简单文档解析（仅文本/markdown）
/// </summary>
public class SimpleDocumentParser(
    IHttpClientFactory httpClientFactory,
    FileStorageService fileStorageService,
    ILogger<SimpleDocumentParser> logger
) : IDocumentParser
{
    public async Task<DocumentParseResult> ParseAsync(RagDocument document, string? rawContent, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(rawContent))
        {
            return ToResult(rawContent, document.ContentType);
        }

        // 优先处理文件路径
        if (!string.IsNullOrWhiteSpace(document.FilePath))
        {
            var localPath = await fileStorageService.ResolveFilePathAsync(
                document.FilePath,
                document.StorageType,
                cancellationToken
            );

            if (localPath != null)
            {
                try
                {
                    var text = await File.ReadAllTextAsync(localPath, cancellationToken);
                    return ToResult(text, document.ContentType);
                }
                finally
                {
                    // 清理临时文件（仅限云存储下载的文件）
                    if (document.StorageType != StorageType.Local)
                    {
                        fileStorageService.CleanupTempFile(localPath);
                    }
                }
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
            return ToResult(text, document.ContentType);
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
