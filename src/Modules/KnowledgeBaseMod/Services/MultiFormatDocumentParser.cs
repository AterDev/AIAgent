using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using OfficeOpenXml;
using System.Text;

namespace KnowledgeBaseMod.Services;

/// <summary>
/// 多格式文档解析器，支持 PDF、Word、Excel 等格式
/// </summary>
public class MultiFormatDocumentParser(
    IHttpClientFactory httpClientFactory,
    ILogger<MultiFormatDocumentParser> logger
) : IDocumentParser
{
    public async Task<DocumentParseResult> ParseAsync(RagDocument document, string? rawContent, CancellationToken cancellationToken = default)
    {
        // 如果提供了原始内容，直接使用
        if (!string.IsNullOrWhiteSpace(rawContent))
        {
            return ToResult(rawContent, document.ContentType);
        }

        // 如果有 SourceUrl，下载并解析
        if (!string.IsNullOrWhiteSpace(document.SourceUrl) && Uri.TryCreate(document.SourceUrl, UriKind.Absolute, out var uri))
        {
            if (uri.Scheme is not ("https" or "http"))
            {
                throw new BusinessException("SourceUrl must be http/https");
            }

            var client = httpClientFactory.CreateClient();
            var bytes = await client.GetByteArrayAsync(uri, cancellationToken);
            
            return await ParseByContentTypeAsync(document.ContentType, bytes, document.FileName, cancellationToken);
        }

        // 如果有文件名，从文件系统读取（这里假设有文件存储）
        // 实际项目中可能需要从 S3、Azure Blob 等存储读取
        logger.LogWarning("No document content available for {DocumentId}", document.Id);
        throw new BusinessException("Document content is empty");
    }

    private async Task<DocumentParseResult> ParseByContentTypeAsync(
        string contentType,
        byte[] bytes,
        string fileName,
        CancellationToken cancellationToken)
    {
        try
        {
            var text = contentType.ToLowerInvariant() switch
            {
                // PDF
                "application/pdf" => ParsePdf(bytes),
                
                // Word
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document" => ParseWord(bytes),
                "application/msword" => ParseWord(bytes),
                
                // Excel
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" => ParseExcel(bytes),
                "application/vnd.ms-excel" => ParseExcel(bytes),
                
                // 纯文本
                "text/plain" => Encoding.UTF8.GetString(bytes),
                "text/markdown" => Encoding.UTF8.GetString(bytes),
                "text/html" => Encoding.UTF8.GetString(bytes),
                
                // 默认尝试文本解析
                _ => TryParseAsText(bytes, contentType, fileName)
            };

            return ToResult(text, contentType);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to parse document with content type {ContentType}", contentType);
            throw new BusinessException($"Failed to parse document: {ex.Message}");
        }
    }

    private string ParsePdf(byte[] bytes)
    {
        using var stream = new MemoryStream(bytes);
        using var reader = new PdfReader(stream);
        using var document = new PdfDocument(reader);
        
        var text = new StringBuilder();
        
        for (int i = 1; i <= document.GetNumberOfPages(); i++)
        {
            var page = document.GetPage(i);
            var strategy = new SimpleTextExtractionStrategy();
            var pageText = PdfTextExtractor.GetTextFromPage(page, strategy);
            text.AppendLine(pageText);
        }
        
        return text.ToString();
    }

    private string ParseWord(byte[] bytes)
    {
        using var stream = new MemoryStream(bytes);
        using var wordDoc = WordprocessingDocument.Open(stream, false);
        
        var body = wordDoc.MainDocumentPart?.Document.Body;
        if (body == null)
        {
            return string.Empty;
        }
        
        var text = new StringBuilder();
        
        foreach (var paragraph in body.Elements<Paragraph>())
        {
            text.AppendLine(paragraph.InnerText);
        }
        
        return text.ToString();
    }

    private string ParseExcel(byte[] bytes)
    {
        // 设置 EPPlus 许可证上下文（非商业使用）
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        
        using var stream = new MemoryStream(bytes);
        using var package = new ExcelPackage(stream);
        
        var text = new StringBuilder();
        
        foreach (var worksheet in package.Workbook.Worksheets)
        {
            text.AppendLine($"工作表: {worksheet.Name}");
            text.AppendLine();
            
            var dimension = worksheet.Dimension;
            if (dimension == null) continue;
            
            for (int row = dimension.Start.Row; row <= dimension.End.Row; row++)
            {
                var rowValues = new List<string>();
                for (int col = dimension.Start.Column; col <= dimension.End.Column; col++)
                {
                    var cell = worksheet.Cells[row, col];
                    var value = cell.Value?.ToString() ?? string.Empty;
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        rowValues.Add(value);
                    }
                }
                
                if (rowValues.Count > 0)
                {
                    text.AppendLine(string.Join("\t", rowValues));
                }
            }
            
            text.AppendLine();
        }
        
        return text.ToString();
    }

    private string TryParseAsText(byte[] bytes, string contentType, string fileName)
    {
        // 根据文件扩展名尝试解析
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        
        return extension switch
        {
            ".pdf" => ParsePdf(bytes),
            ".docx" => ParseWord(bytes),
            ".doc" => ParseWord(bytes),
            ".xlsx" => ParseExcel(bytes),
            ".xls" => ParseExcel(bytes),
            ".txt" or ".md" or ".html" or ".htm" => Encoding.UTF8.GetString(bytes),
            _ => throw new BusinessException($"Unsupported file type: {contentType} / {extension}")
        };
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

        // 简单估算：平均 4 个字符约等于 1 个 token
        return Math.Max(1, text.Length / 4);
    }
}
