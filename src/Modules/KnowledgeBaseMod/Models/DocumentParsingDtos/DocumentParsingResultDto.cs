using Entity.KnowledgeBaseMod;

namespace KnowledgeBaseMod.Models.DocumentParsingDtos;

public class DocumentParsingResultDto
{
    public Guid Id { get; set; }
    public Guid RagDocumentId { get; set; }
    public DocumentParsingStatus ParsingStatus { get; set; }
    public int WordCount { get; set; }
    public int? PageCount { get; set; }
    public long? DurationMs { get; set; }
    public DateTime? CompletedTime { get; set; }
    public DateTime CreatedAt { get; set; }
}
