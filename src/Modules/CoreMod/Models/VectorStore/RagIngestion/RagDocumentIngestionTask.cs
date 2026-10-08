namespace CoreMod.Models.RagIngestion;

public record RagDocumentIngestionTask(Guid DocumentId, Guid TenantId, string? ContentText);
