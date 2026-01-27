using System.Net.Http;

namespace KnowledgeBaseMod.Services;

/// <summary>
/// 简单文档解析（仅文本/markdown）
/// </summary>
public class SimpleDocumentParser(
    IHttpClientFactory httpClientFactory,
    ILogger<SimpleDocumentParser> logger
) : IDocumentParser
{
    public async Task<DocumentParseResult> ParseAsync(RagDocument document, string? rawContent, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(rawContent))
        {
            return ToResult(rawContent, document.ContentType);
        }

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
