namespace KnowledgeBaseMod.Services;

public class DocumentParseResult
{
    public string Text { get; set; } = string.Empty;

    public int TokenCount { get; set; }

    public string ContentType { get; set; } = string.Empty;

    public List<ParsedImage> Images { get; set; } = [];

    public List<ParsedAttachment> Attachments { get; set; } = [];

    public Dictionary<string, string> Metadata { get; set; } = [];
}

public class ParsedImage
{
    public required string FileName { get; set; }

    public required byte[] Data { get; set; }

    public required string ContentType { get; set; }

    public string? Caption { get; set; }

    public int? Width { get; set; }

    public int? Height { get; set; }
}

public class ParsedAttachment
{
    public required string FileName { get; set; }

    public required byte[] Data { get; set; }

    public required string ContentType { get; set; }

    public string? Description { get; set; }
}
