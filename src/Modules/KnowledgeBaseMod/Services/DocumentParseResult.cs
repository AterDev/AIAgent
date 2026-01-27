namespace KnowledgeBaseMod.Services;

public class DocumentParseResult
{
    public string Text { get; set; } = string.Empty;

    public int TokenCount { get; set; }

    public string ContentType { get; set; } = string.Empty;
}
