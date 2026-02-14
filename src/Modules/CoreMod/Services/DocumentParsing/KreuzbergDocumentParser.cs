using DocumentFormat.OpenXml.Packaging;
using DText = DocumentFormat.OpenXml.Drawing.Text;
using WText = DocumentFormat.OpenXml.Wordprocessing.Text;
using OfficeOpenXml;
using System.Text;
using Tesseract;
using UglyToad.PdfPig;
using Entity.KnowledgeBaseMod;

namespace CoreMod.Services.DocumentParsing;

/// <summary>
/// Kreuzberg-based document parser supporting 57+ formats with OCR and table extraction
/// TODO: Will be fully implemented after Kreuzberg package installation and API review
/// </summary>
public class KreuzbergDocumentParser(
    IHttpClientFactory httpClientFactory,
    IFileStorageService fileStorageService,
    IStorageProviderQuery storageProviderQuery,
    ILogger<KreuzbergDocumentParser> logger
) : IDocumentParser
{
    public async Task<DocumentParseResult> ParseAsync(
        RagDocument document,
        string? rawContent,
        CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(rawContent))
        {
            return ToResult(rawContent, document.FileType);
        }

        byte[] fileBytes;
        string? localPath = null;

        if (!string.IsNullOrWhiteSpace(document.FilePath))
        {
            if (File.Exists(document.FilePath))
            {
                localPath = document.FilePath;
                fileBytes = await File.ReadAllBytesAsync(localPath, cancellationToken);
            }
            else if (document.StorageProviderId != Guid.Empty)
            {
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
                    throw new BusinessException($"File not found: {document.FilePath}");
                }
            }
            else
            {
                throw new BusinessException($"File not found: {document.FilePath}");
            }
        }
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
            "xlsx" => ParseExcel(fileBytes),
            "csv" => ParseCsv(fileBytes),
            "doc" or "ppt" or "xls" => throw new BusinessException("Legacy Office formats (.doc/.ppt/.xls) are not supported. Please use .docx/.pptx/.xlsx."),
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

    private static string ParseExcel(byte[] fileBytes)
    {
        ExcelPackage.License.SetNonCommercialOrganization("AIAgent");
        using var stream = new MemoryStream(fileBytes);
        using var package = new ExcelPackage(stream);
        var builder = new StringBuilder();

        foreach (var worksheet in package.Workbook.Worksheets)
        {
            if (worksheet.Dimension == null)
            {
                continue;
            }

            builder.AppendLine($"## {worksheet.Name}");
            builder.AppendLine();

            var startRow = worksheet.Dimension.Start.Row;
            var endRow = worksheet.Dimension.End.Row;
            var startCol = worksheet.Dimension.Start.Column;
            var endCol = worksheet.Dimension.End.Column;

            for (var row = startRow; row <= endRow; row++)
            {
                var cells = new List<string>();
                for (var col = startCol; col <= endCol; col++)
                {
                    var value = worksheet.Cells[row, col].Text ?? string.Empty;
                    cells.Add(value);
                }

                var line = string.Join(" | ", cells);
                if (!string.IsNullOrWhiteSpace(line.Replace("|", "").Trim()))
                {
                    builder.AppendLine(line);
                }
            }

            builder.AppendLine();
        }

        return builder.ToString();
    }

    private static string ParseCsv(byte[] fileBytes)
    {
        var text = Encoding.UTF8.GetString(fileBytes);
        var builder = new StringBuilder();

        var row = new List<string>();
        var cell = new StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];
            if (c == '"')
            {
                if (inQuotes && i + 1 < text.Length && text[i + 1] == '"')
                {
                    cell.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }

                continue;
            }

            if (!inQuotes && c == ',')
            {
                row.Add(cell.ToString());
                cell.Clear();
                continue;
            }

            if (!inQuotes && (c == '\n' || c == '\r'))
            {
                if (c == '\r' && i + 1 < text.Length && text[i + 1] == '\n')
                {
                    i++;
                }

                row.Add(cell.ToString());
                cell.Clear();

                if (RowHasContent(row))
                {
                    builder.AppendLine(string.Join(" | ", row));
                }

                row.Clear();
                continue;
            }

            cell.Append(c);
        }

        if (cell.Length > 0 || row.Count > 0)
        {
            row.Add(cell.ToString());
            if (RowHasContent(row))
            {
                builder.AppendLine(string.Join(" | ", row));
            }
        }

        return builder.ToString();
    }

    private static bool RowHasContent(List<string> row)
    {
        return row.Any(cell => !string.IsNullOrWhiteSpace(cell));
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

        return Math.Max(1, text.Length / 4);
    }
}