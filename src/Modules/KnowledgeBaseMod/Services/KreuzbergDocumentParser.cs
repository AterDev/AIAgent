using Perigon.AspNetCore.Toolkit.Services;

namespace KnowledgeBaseMod.Services;

/// <summary>
/// Kreuzberg-based document parser supporting 57+ formats with OCR and table extraction
/// TODO: Will be fully implemented after Kreuzberg package installation and API review
/// </summary>
public class KreuzbergDocumentParser(
    IHttpClientFactory httpClientFactory,
    FileStorageService fileStorageService,
    ILogger<KreuzbergDocumentParser> logger
) : IDocumentParser
{
    public async Task<DocumentParseResult> ParseAsync(
        RagDocument document,
        string? rawContent,
        CancellationToken cancellationToken = default)
    {
        // If raw content is provided, parse it directly as plain text
        if (!string.IsNullOrWhiteSpace(rawContent))
        {
            return ToResult(rawContent, document.ContentType);
        }

        byte[] fileBytes;

        // Priority 1: Load from file path if available
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
                    fileBytes = await File.ReadAllBytesAsync(localPath, cancellationToken);
                }
                finally
                {
                    // Clean up temp file (only for cloud storage)
                    if (document.StorageType != StorageType.Local)
                    {
                        fileStorageService.CleanupTempFile(localPath);
                    }
                }
            }
            else
            {
                throw new BusinessException($"File not found: {document.FilePath}");
            }
        }
        // Priority 2: Download from URL if provided
        else if (!string.IsNullOrWhiteSpace(document.SourceUrl) && 
            Uri.TryCreate(document.SourceUrl, UriKind.Absolute, out var uri))
        {
            if (uri.Scheme is not ("https" or "http"))
            {
                throw new BusinessException("SourceUrl must be http/https");
            }

            var client = httpClientFactory.CreateClient();
            fileBytes = await client.GetByteArrayAsync(uri, cancellationToken);
        }
        else
        {
            logger.LogWarning("No document content available for {DocumentId}", document.Id);
            throw new BusinessException("Document content is empty");
        }

        try
        {
            // TODO: Implement actual Kreuzberg parsing after package installation
            // For now, return a placeholder result indicating parsing is not yet implemented
            logger.LogWarning("Kreuzberg parser not yet fully implemented, returning placeholder for document {DocumentId}", document.Id);
            
            return new DocumentParseResult
            {
                Text = $"# Document Parsing Pending\n\nThis document will be parsed using Kreuzberg once the implementation is complete.\n\nFile: {document.FileName}\nType: {document.ContentType}\nSize: {fileBytes.Length} bytes",
                TokenCount = EstimateTokens($"Document pending: {document.FileName}"),
                ContentType = document.ContentType,
                Metadata = new Dictionary<string, string>
                {
                    ["status"] = "pending_implementation",
                    ["parser"] = "kreuzberg",
                    ["file_size"] = fileBytes.Length.ToString()
                }
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Parsing failed for document {DocumentId} ({ContentType})", 
                document.Id, document.ContentType);
            throw new BusinessException($"Failed to parse document: {ex.Message}");
        }
    }

    private static DocumentParseResult ToResult(string text, string? contentType)
    {
        return new DocumentParseResult
        {
            Text = text,
            TokenCount = EstimateTokens(text),
            ContentType = contentType ?? "text/plain"
        };
    }

    private static int EstimateTokens(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return 0;
        }

        // Simple estimation: ~4 characters per token
        return Math.Max(1, text.Length / 4);
    }
}
