using DocumentFormat.OpenXml.Packaging;
using DText = DocumentFormat.OpenXml.Drawing.Text;
using WText = DocumentFormat.OpenXml.Wordprocessing.Text;
using Perigon.AspNetCore.Toolkit.Services;
using System.Text;
using Tesseract;
using UglyToad.PdfPig;
using Entity.KnowledgeBaseMod;
using Share.Services;
using Share.Exceptions;

namespace CoreMod.Services;

/// <summary>
/// Kreuzberg-based document parser supporting 57+ formats with OCR and table extraction
/// TODO: Will be fully implemented after Kreuzberg package installation and API review
/// </summary>
public class KreuzbergDocumentParser(
    IHttpClientFactory httpClientFactory,
    IFileStorageService fileStorageService,
    IStorageProviderQuery storageProviderQuery,
    ILogger<KreuzbergDocumentParser> logger
) : Share.Services.IDocumentParser
{
    public async Task<DocumentParseResult> ParseAsync(
        RagDocument document,
        string? rawContent,
        CancellationToken cancellationToken = default)
    {
        // If raw content is provided, parse it directly as plain text
        if (!string.IsNullOrWhiteSpace(rawContent))
        {
            return ToResult(rawContent, document.FileType);
        }

        byte[] fileBytes;
        string? localPath = null;

        // Priority 1: Load from file path if available
        if (!string.IsNullOrWhiteSpace(document.FilePath))
        {
            if (File.Exists(document.FilePath))
            {
                localPath = document.FilePath;
                fileBytes = await File.ReadAllBytesAsync(localPath, cancellationToken);
            }
            else if (document.StorageProviderId != Guid.Empty)
            {
                // Query storage provider info
                var provider = await storageProviderQuery.GetProviderAsync(document.StorageProviderId, cancellationToken);
                if (provider == null)
                {
                    logger.LogWarning("Storage provider not found: {StorageProviderId}", document.StorageProviderId);
                    throw new BusinessException($"Storage provider not found: {document.StorageProviderId}");
                }

                localPath = await fileStorageService.DownloadFileAsync(
                    document.StorageProviderId,
                    document.FilePath,
                    cancellationToken
                );

                if (localPath != null)
                {
                    try
                    {
                        fileBytes = await File.ReadAllBytesAsync(localPath, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Failed to read file {FilePath}", localPath);
                        throw new BusinessException($"Failed to read file: {ex.Message}");
                    }
                    finally
                    {
                        // Clean up temp file (only for cloud storage)
                        if (provider.IsCloud)
                        {
                            try
                            {
                                fileStorageService.CleanupTempFile(localPath);
                            }
                            catch (Exception ex)
                            {
                                logger.LogWarning(ex, "Failed to cleanup temp file {TempPath}", localPath);
                                // 不再抛出异常，避免掩盖原始错误
                            }
                        }
                    }
                }
                else
                {
                    throw new BusinessException($"File not found: {document.FilePath}");
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
            var fileType = NormalizeFileType(document);
            var text = ParseContent(fileType, fileBytes, localPath);
            return ToResult(text, fileType);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Parsing failed for document {DocumentId} ({FileType})", 
                document.Id, document.FileType);
            throw new BusinessException($"Failed to parse document: {ex.Message}");
        }
    }

    private static string NormalizeFileType(RagDocument document)
    {
        if (!string.IsNullOrWhiteSpace(document.FileType))
        {
            return document.FileType.Trim().TrimStart('.').ToLowerInvariant();
        }

        if (!string.IsNullOrWhiteSpace(document.FileName))
        {
            return Path.GetExtension(document.FileName).TrimStart('.').ToLowerInvariant();
        }

        return "txt";
    }

    private static string ParseContent(string fileType, byte[] fileBytes, string? filePath)
    {
        return fileType switch
        {
            "pdf" => ParsePdf(fileBytes),
            "docx" => ParseDocx(fileBytes),
            "pptx" => ParsePptx(fileBytes),
            "doc" or "ppt" => throw new BusinessException("Legacy Office formats are not supported. Please use .docx or .pptx."),
            "jpg" or "jpeg" or "png" => ParseImageWithOcr(fileBytes, filePath),
            _ => ParseText(fileBytes)
        };
    }

    private static string ParseText(byte[] fileBytes)
    {
        return Encoding.UTF8.GetString(fileBytes);
    }

    private static string ParsePdf(byte[] fileBytes)
    {
        using var stream = new MemoryStream(fileBytes);
        using var document = PdfDocument.Open(stream);
        var builder = new StringBuilder();
        foreach (var page in document.GetPages())
        {
            if (!string.IsNullOrWhiteSpace(page.Text))
            {
                builder.AppendLine(page.Text.Trim());
            }
        }

        return builder.ToString();
    }

    private static string ParseDocx(byte[] fileBytes)
    {
        using var stream = new MemoryStream(fileBytes);
        using var document = WordprocessingDocument.Open(stream, false);
        var body = document.MainDocumentPart?.Document.Body;
        if (body == null)
        {
            return string.Empty;
        }

        var builder = new StringBuilder();
        foreach (var text in body.Descendants<WText>())
        {
            if (!string.IsNullOrWhiteSpace(text.Text))
            {
                builder.AppendLine(text.Text.Trim());
            }
        }

        return builder.ToString();
    }

    private static string ParsePptx(byte[] fileBytes)
    {
        using var stream = new MemoryStream(fileBytes);
        using var presentation = PresentationDocument.Open(stream, false);
        var slideParts = presentation.PresentationPart?.SlideParts;
        if (slideParts == null)
        {
            return string.Empty;
        }

        var builder = new StringBuilder();
        foreach (var slidePart in slideParts)
        {
            foreach (var text in slidePart.Slide.Descendants<DText>())
            {
                if (!string.IsNullOrWhiteSpace(text.Text))
                {
                    builder.AppendLine(text.Text.Trim());
                }
            }
        }

        return builder.ToString();
    }

    private static string ParseImageWithOcr(byte[] fileBytes, string? filePath)
    {
        var tessdataPath = ResolveTessdataPath();
        using var engine = new TesseractEngine(tessdataPath, "eng", EngineMode.Default);
        using var pix = filePath != null && File.Exists(filePath)
            ? Pix.LoadFromFile(filePath)
            : Pix.LoadFromMemory(fileBytes);
        using var page = engine.Process(pix);
        return page.GetText() ?? string.Empty;
    }

    private static string ResolveTessdataPath()
    {
        var envPath = Environment.GetEnvironmentVariable("TESSDATA_PREFIX");
        if (!string.IsNullOrWhiteSpace(envPath))
        {
            var normalized = envPath.Trim();
            if (Directory.Exists(Path.Combine(normalized, "tessdata")))
            {
                return Path.Combine(normalized, "tessdata");
            }

            if (Directory.Exists(normalized))
            {
                return normalized;
            }
        }

        var candidates = new[]
        {
            @"C:\\Program Files\\Tesseract-OCR\\tessdata",
            @"C:\\Program Files (x86)\\Tesseract-OCR\\tessdata"
        };

        foreach (var candidate in candidates)
        {
            if (Directory.Exists(candidate))
            {
                return candidate;
            }
        }

        throw new BusinessException("Tesseract data not found. Set TESSDATA_PREFIX to the tessdata folder.");
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

        // Simple estimation: ~4 characters per token
        return Math.Max(1, text.Length / 4);
    }
}
