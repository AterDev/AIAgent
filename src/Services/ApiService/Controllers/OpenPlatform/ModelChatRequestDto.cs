using CoreMod.Models;

namespace ApiService.Controllers.OpenPlatform;

public sealed class ModelChatRequestDto
{
    public Guid? ApplicationId { get; set; }

    public string Model { get; set; } = string.Empty;

    public string? Provider { get; set; }

    public string? Scene { get; set; }

    public List<ModelMessage> Messages { get; set; } = [];

    public double? Temperature { get; set; }

    public int? MaxTokens { get; set; }
}