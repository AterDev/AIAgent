using Perigon.AspNetCore.Toolkit.Services;

namespace KnowledgeBaseMod.Services;

/// <summary>
/// Kreuzberg-based document parser supporting 57+ formats with OCR and table extraction
/// TODO: Will be fully implemented after Kreuzberg package installation and API review
/// </summary>
public class KreuzbergDocumentParser(
    IHttpClientFactory httpClientFactory,
    AWSS3Service s3Service,
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

        // Download from URL if provided
        if (!string.IsNullOrWhiteSpace(document.SourceUrl) && 
            Uri.TryCreate(document.SourceUrl, UriKind.Absolute, out var uri))
        {
            if (uri.Scheme is not ("https" or "http"))
            {
                throw new BusinessException("SourceUrl must be http/https");
            }

            var client = httpClientFactory.CreateClient();
            fileBytes = await client.GetByteArrayAsync(uri, cancellationToken);
        }
        // Load from file path if available (S3 or local)
        else if (!string.IsNullOrWhiteSpace(document.FilePath))
        {
            // Try S3 first, then local file system
            if (document.FilePath.StartsWith("s3://", StringComparison.OrdinalIgnoreCase))
            {
                fileBytes = await GetS3FileAsync(document.FilePath, cancellationToken);
            }
            else if (File.Exists(document.FilePath))
            {
                fileBytes = await File.ReadAllBytesAsync(document.FilePath, cancellationToken);
            }
            else
            {
                throw new BusinessException($"File not found: {document.FilePath}");
            }
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

    private async Task<byte[]> GetS3FileAsync(string s3Path, CancellationToken cancellationToken)
    {
        // Extract key from s3:// URL
        var key = s3Path.Replace("s3://", "").TrimStart('/');
        
        var response = await s3Service.GetObjectAsync(key, cancellationToken);
        if (response == null)
        {
            throw new BusinessException($"S3 object not found: {s3Path}");
        }

        using var memoryStream = new MemoryStream();
        await response.ResponseStream.CopyToAsync(memoryStream, cancellationToken);
        return memoryStream.ToArray();
    }

    private async Task<string> UploadToS3Async(
        string key,
        byte[] data,
        CancellationToken cancellationToken)
    {
        using var stream = new MemoryStream(data);
        var success = await s3Service.UploadAsync(key, stream, cancellationToken);
        
        if (!success)
        {
            throw new BusinessException($"Failed to upload to S3: {key}");
        }

        // Return the S3 URL
        return $"s3://{s3Service.BucketName}/{key}";
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

    private static string GetExtensionFromContentType(string contentType)
    {
        return contentType.ToLowerInvariant() switch
        {
            "image/png" => ".png",
            "image/jpeg" or "image/jpg" => ".jpg",
            "image/gif" => ".gif",
            "image/bmp" => ".bmp",
            "image/tiff" => ".tiff",
            "image/webp" => ".webp",
            "image/svg+xml" => ".svg",
            _ => ".bin"
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
