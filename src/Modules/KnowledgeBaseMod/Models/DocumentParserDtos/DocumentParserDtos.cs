using Entity.KnowledgeBaseMod;

namespace KnowledgeBaseMod.Models.DocumentParserDtos;

public class DocumentParsingBaseDto
{
    public Guid RagDocumentId { get; set; }
    public DocumentFormatType DocumentFormat { get; set; }
    public DocumentParsingStatus ParsingStatus { get; set; }
}

public class CreateDocumentParsingDto
{
    public Guid RagDocumentId { get; set; }
    public DocumentFormatType DocumentFormat { get; set; }
    public required string FilePath { get; set; }
    public byte[]? FileData { get; set; }
}

public class DocumentParsingRequestDto
{
    public Guid ParsingResultId { get; set; }
    public required string FileContent { get; set; }
    public DocumentFormatType DocumentFormat { get; set; }
    public string? FileName { get; set; }
}

public class DocumentParsingResultDto
{
    public Guid Id { get; set; }
    public Guid RagDocumentId { get; set; }
    public DocumentParsingStatus ParsingStatus { get; set; }
    public string? TextContent { get; set; }
    public int? PageCount { get; set; }
    public int WordCount { get; set; }
    public string? ErrorMessage { get; set; }
    public long? DurationMs { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedTime { get; set; }
}

public class BatchDocumentParsingDto
{
    public required List<DocumentParsingRequestDto> Documents { get; set; }
    public DocumentProcessingOptionsDto? Options { get; set; }
}

public class DocumentProcessingOptionsDto
{
    public int MaxConcurrency { get; set; } = 3;
    public bool IgnoreFormatting { get; set; } = false;
    public int TimeoutSeconds { get; set; } = 30;
    public bool CleanSpecialCharacters { get; set; } = true;
}
