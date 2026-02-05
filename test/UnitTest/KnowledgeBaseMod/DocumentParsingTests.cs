using DocumentFormat.OpenXml.Packaging;
using D = DocumentFormat.OpenXml.Drawing;
using P = DocumentFormat.OpenXml.Presentation;
using W = DocumentFormat.OpenXml.Wordprocessing;
using Entity.KnowledgeBaseMod;
using Share.Services;
using CoreMod.Services;
using Microsoft.Extensions.Logging.Abstractions;
using SkiaSharp;
using UglyToad.PdfPig;
using System.Net.Http;
using System.Text;

namespace UnitTest.KnowledgeBaseMod;

/// <summary>
/// 文档解析单元测试（PDF/Word/PowerPoint/Image OCR）
/// </summary>
public class DocumentParsingTests
{
    private static readonly IHttpClientFactory HttpClientFactory = new FakeHttpClientFactory();
    private static readonly IFileStorageService FileStorageService = new FakeFileStorageService();

    [Test]
    public async Task ParsePdf_ShouldExtractText()
    {
        var expected = "Hello PDF";
        var pdfBytes = CreatePdf(expected);
        try
        {
            using var _ = UglyToad.PdfPig.PdfDocument.Open(new MemoryStream(pdfBytes));
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(ex.ToString());
        }

        var filePath = WriteTempFile(".pdf", pdfBytes);
        try
        {
            var result = await ParseWithKreuzbergAsync(filePath, "pdf");
            await Assert.That(result.Text).Contains(expected);
        }
        finally
        {
            DeleteIfExists(filePath);
        }
    }

    [Test]
    public async Task ParseDocx_ShouldExtractText()
    {
        var expected = "Hello DOCX";
        var filePath = WriteTempFile(".docx", CreateDocx(expected));
        try
        {
            var result = await ParseWithKreuzbergAsync(filePath, "docx");
            await Assert.That(result.Text).Contains(expected);
        }
        finally
        {
            DeleteIfExists(filePath);
        }
    }

    [Test]
    public async Task ParsePptx_ShouldExtractText()
    {
        var expected = "Hello PPTX";
        var filePath = WriteTempFile(".pptx", CreatePptx(expected));
        try
        {
            var result = await ParseWithKreuzbergAsync(filePath, "pptx");
            await Assert.That(result.Text).Contains(expected);
        }
        finally
        {
            DeleteIfExists(filePath);
        }
    }

    [Test]
    public async Task ParsePngWithOcr_ShouldExtractText()
    {
        var expected = "HELLO OCR";
        if (!EnsureTessdataPath())
        {
            return;
        }
        var filePath = WriteTempFile(".png", CreatePng(expected));
        try
        {
            var result = await ParseWithKreuzbergAsync(filePath, "png");
            await Assert.That(result.Text).Contains("HELLO");
        }
        finally
        {
            DeleteIfExists(filePath);
        }
    }

    private static async Task<DocumentParseResult> ParseWithKreuzbergAsync(string filePath, string fileType)
    {
        var parser = new KreuzbergDocumentParser(
            HttpClientFactory,
            FileStorageService,
            new NullStorageProviderQuery(),
            NullLogger<KreuzbergDocumentParser>.Instance);

        var document = new RagDocument
        {
            Id = Guid.NewGuid(),
            Name = $"Parse {Guid.NewGuid():N}",
            FilePath = filePath,
            FileName = Path.GetFileName(filePath),
            FileType = fileType,
            StorageProviderId = Guid.Empty
        };

        return await parser.ParseAsync(document, null, CancellationToken.None);
    }

    private static string WriteTempFile(string extension, byte[] content)
    {
        var path = Path.Combine(Path.GetTempPath(), $"rag_parse_{Guid.NewGuid():N}{extension}");
        File.WriteAllBytes(path, content);
        return path;
    }

    private static void DeleteIfExists(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    private static byte[] CreateDocx(string text)
    {
        using var stream = new MemoryStream();
        using (var document = WordprocessingDocument.Create(stream, DocumentFormat.OpenXml.WordprocessingDocumentType.Document, true))
        {
            var mainPart = document.AddMainDocumentPart();
            mainPart.Document = new W.Document();
            var body = mainPart.Document.AppendChild(new W.Body());
            var paragraph = body.AppendChild(new W.Paragraph());
            var run = paragraph.AppendChild(new W.Run());
            run.AppendChild(new W.Text(text));
            mainPart.Document.Save();
        }

        return stream.ToArray();
    }

    private static byte[] CreatePptx(string text)
    {
        using var stream = new MemoryStream();
        using (var presentation = PresentationDocument.Create(stream, DocumentFormat.OpenXml.PresentationDocumentType.Presentation, true))
        {
            var presentationPart = presentation.AddPresentationPart();
            presentationPart.Presentation = new P.Presentation();

            var slidePart = presentationPart.AddNewPart<SlidePart>();
            var textBody = new P.TextBody(
                new D.BodyProperties(),
                new D.ListStyle(),
                new D.Paragraph(new D.Run(new D.Text(text))));

            var textShape = new P.Shape(
                new P.NonVisualShapeProperties(
                    new P.NonVisualDrawingProperties { Id = 2U, Name = "Content" },
                    new P.NonVisualShapeDrawingProperties(new D.ShapeLocks { NoGrouping = true }),
                    new P.ApplicationNonVisualDrawingProperties()),
                new P.ShapeProperties(),
                textBody);

            var shapeTree = new P.ShapeTree(
                new P.NonVisualGroupShapeProperties(
                    new P.NonVisualDrawingProperties { Id = 1U, Name = string.Empty },
                    new P.NonVisualGroupShapeDrawingProperties(),
                    new P.ApplicationNonVisualDrawingProperties()),
                new P.GroupShapeProperties(new D.TransformGroup()),
                textShape);

            slidePart.Slide = new P.Slide(
                new P.CommonSlideData(shapeTree),
                new P.ColorMapOverride(new D.MasterColorMapping()));

            slidePart.Slide.Save();

            var slideIdList = presentationPart.Presentation.AppendChild(new P.SlideIdList());
            slideIdList.AppendChild(new P.SlideId
            {
                Id = 256U,
                RelationshipId = presentationPart.GetIdOfPart(slidePart)
            });

            presentationPart.Presentation.Save();
        }

        return stream.ToArray();
    }

    private static byte[] CreatePng(string text)
    {
        var info = new SKImageInfo(800, 200);
        using var surface = SKSurface.Create(info);
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.White);
        using var paint = new SKPaint
        {
            Color = SKColors.Black,
            IsAntialias = true
        };
        using var font = new SKFont(SKTypeface.FromFamilyName("Arial"), 72);
        canvas.DrawText(text, 20, 120, SKTextAlign.Left, font, paint);
        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }

    private static byte[] CreatePdf(string text)
    {
        var newLine = "\r\n";
        var escapedText = EscapePdfText(text);
        var contentStream = $"BT /F1 24 Tf 72 720 Td ({escapedText}) Tj ET";

        var objects = new List<string>
        {
            $"1 0 obj{newLine}<< /Type /Catalog /Pages 2 0 R >>{newLine}endobj{newLine}",
            $"2 0 obj{newLine}<< /Type /Pages /Count 1 /Kids [3 0 R] >>{newLine}endobj{newLine}",
            $"3 0 obj{newLine}<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Contents 4 0 R /Resources << /Font << /F1 5 0 R >> >> >>{newLine}endobj{newLine}",
            $"4 0 obj{newLine}<< /Length {Encoding.ASCII.GetByteCount(contentStream)} >>{newLine}stream{newLine}{contentStream}{newLine}endstream{newLine}endobj{newLine}",
            $"5 0 obj{newLine}<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>{newLine}endobj{newLine}"
        };

        var builder = new StringBuilder();
        builder.Append($"%PDF-1.4{newLine}");

        var offsets = new List<int> { 0 };
        foreach (var obj in objects)
        {
            offsets.Add(Encoding.ASCII.GetByteCount(builder.ToString()));
            builder.Append(obj);
        }

        var xrefStart = Encoding.ASCII.GetByteCount(builder.ToString());
        builder.Append($"xref{newLine}");
        builder.Append($"0 {objects.Count + 1}{newLine}");
        builder.Append($"0000000000 65535 f {newLine}");
        foreach (var offset in offsets.Skip(1))
        {
            builder.Append($"{offset:D10} 00000 n {newLine}");
        }

        builder.Append($"trailer{newLine}");
        builder.Append($"<< /Size {objects.Count + 1} /Root 1 0 R >>{newLine}");
        builder.Append($"startxref{newLine}");
        builder.Append($"{xrefStart}{newLine}");
        builder.Append($"%%EOF{newLine}");

        return Encoding.ASCII.GetBytes(builder.ToString());
    }

    private static string EscapePdfText(string text)
    {
        return text.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");
    }

    private static bool EnsureTessdataPath()
    {
        var envPath = Environment.GetEnvironmentVariable("TESSDATA_PREFIX");
        if (!string.IsNullOrWhiteSpace(envPath))
        {
            var normalized = envPath.Trim();
            if (Directory.Exists(Path.Combine(normalized, "tessdata")) || Directory.Exists(normalized))
            {
                return true;
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
                Environment.SetEnvironmentVariable("TESSDATA_PREFIX", candidate);
                return true;
            }
        }

        return false;
    }

    private sealed class FakeHttpClientFactory : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => new();
    }

    private sealed class FakeFileStorageService : IFileStorageService
    {
        public Task<FileUploadResult> UploadAsync(Stream stream, string fileName, string category, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<bool> DeleteAsync(string filePath, bool isCloud, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public string? GetSignedUrl(string objectKey, int expiresSeconds = 86400)
            => null;

        public Task<string?> DownloadFileAsync(Guid storageProviderId, string objectKey, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public void CleanupTempFile(string tempFilePath)
        {
        }
    }

    private sealed class NullStorageProviderQuery : IStorageProviderQuery
    {
        public Task<StorageProviderInfo?> GetProviderAsync(Guid storageProviderId, CancellationToken cancellationToken = default)
            => Task.FromResult<StorageProviderInfo?>(null);

        public Task<StorageProviderInfo?> GetActiveProviderAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<StorageProviderInfo?>(null);
    }
}
