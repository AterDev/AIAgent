namespace Share.Services;

public interface IDocumentParser
{
    Task<DocumentParseResult> ParseAsync(RagDocument document, string? rawContent, CancellationToken cancellationToken = default);
}

public record DocumentParseResult(string Text, int TokenCount);
