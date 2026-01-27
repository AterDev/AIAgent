namespace KnowledgeBaseMod.Services;

public interface IDocumentParser
{
    Task<DocumentParseResult> ParseAsync(RagDocument document, string? rawContent, CancellationToken cancellationToken = default);
}
