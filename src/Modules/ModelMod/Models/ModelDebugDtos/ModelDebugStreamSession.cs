namespace ModelMod.Models.ModelDebugDtos;

public sealed class ModelDebugStreamSession
{
    public required string RequestId { get; init; }

    public required string ModelName { get; init; }

    public required IAsyncEnumerable<ModelStreamChunk> Stream { get; init; }
}
