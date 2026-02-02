namespace ModelMod.Models.ModelDebugDtos;

public sealed class ModelDebugStreamEvent
{
    public string Type { get; set; } = "delta";

    public string? RequestId { get; set; }

    public string? Delta { get; set; }

    public ModelDebugResponse? Final { get; set; }

    public string? Error { get; set; }
}
