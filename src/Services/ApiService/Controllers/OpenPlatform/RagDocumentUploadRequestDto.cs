namespace ApiService.Controllers.OpenPlatform;

public sealed class RagDocumentUploadRequestDto
{
    public required IFormFile File { get; set; }

    public Guid CollectionId { get; set; }

    public string? Name { get; set; }

    public List<string>? Tags { get; set; }

    public List<string>? Roles { get; set; }

    public bool AutoParse { get; set; } = true;
}