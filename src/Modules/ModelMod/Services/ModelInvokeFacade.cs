using CoreMod.Models;

namespace ModelMod.Services;

/// <summary>
/// Share 接口适配器（Model Invoke）
/// </summary>
public class ModelInvokeFacade(IModelInvokeService service) : Share.Services.IModelInvokeFacade
{
    public async Task<Share.Services.ModelInvokeResponse> ChatAsync(Guid applicationId, Share.Services.ModelInvokeRequest request, CancellationToken cancellationToken = default)
    {
        var response = await service.ChatAsync(applicationId, MapRequest(request), cancellationToken);
        return MapResponse(response);
    }

    public async Task<Share.Services.ModelInvokeResponse> EmbeddingAsync(Guid applicationId, Share.Services.ModelInvokeRequest request, CancellationToken cancellationToken = default)
    {
        var response = await service.EmbeddingAsync(applicationId, MapRequest(request), cancellationToken);
        return MapResponse(response);
    }

    private static ModelRequest MapRequest(Share.Services.ModelInvokeRequest request)
    {
        return new ModelRequest
        {
            Model = request.Model,
            Provider = request.Provider,
            Scene = request.Scene,
            Messages = request.Messages.Select(m => new ModelMessage { Role = m.Role, Content = m.Content }).ToList(),
            Metadata = request.Metadata,
        };
    }

    private static Share.Services.ModelInvokeResponse MapResponse(ModelResponse response)
    {
        return new Share.Services.ModelInvokeResponse
        {
            Success = response.Success,
            Content = response.Content,
            ErrorMessage = response.ErrorMessage,
            Usage = new Share.Services.UsageStats
            {
                PromptTokens = response.Usage.PromptTokens,
                CompletionTokens = response.Usage.CompletionTokens,
                TotalTokens = response.Usage.TotalTokens,
            },
        };
    }
}
