using Entity.KnowledgeBaseMod;

namespace CoreMod.Services.DocumentParsing;

/// <summary>
/// 简单文档解析（仅文本/markdown）
/// </summary>
public class SimpleDocumentParser(
    IHttpClientFactory httpClientFactory,
    IFileStorageService fileStorageService,
    IStorageProviderQuery storageProviderQuery,
    ILogger<SimpleDocumentParser> logger
) : IDocumentParser
{
    public async Task<DocumentParseResult> ParseAsync(RagDocument document, string? rawContent, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(rawContent))
        {
            return ToResult(rawContent, document.FileType);
        }

        if (!string.IsNullOrWhiteSpace(document.FilePath) && document.StorageProviderId != Guid.Empty)
        {
            var provider = await storageProviderQuery.GetProviderAsync(document.StorageProviderId, cancellationToken);
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
                    var fileType = document.FileType.ToLower();
                    if (fileType is "jpeg" or "jpg" or "png")
                    {
                        throw new BusinessException(
                            $"Image documents ({fileType.ToUpper()}) are not yet supported. " +
                            "Please upload documents in PDF or text format, or wait for OCR support in a future release.");
                    }

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
        return new DocumentParseResult(text, EstimateTokens(text));
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