using CoreMod.Services;
using CoreMod.Models;

namespace ModelMod.Services;

/// <summary>
/// IModelInvoker 的 ModelMod 实现
/// 将 CoreMod 的 ModelInvokeRequest/ModelInvokeResponse 适配到 ModelMod 的 ModelInvokeService
/// </summary>
public class ModelInvoker(
    ModelInvokeService modelInvokeService,
    ILogger<ModelInvoker> logger
) : IModelInvoker
{
    public async Task<ModelInvokeResponse> ChatAsync(Guid applicationId, ModelInvokeRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            // 将 CoreMod DTO 转换为 ModelMod DTO
            var modelRequest = new ModelRequest
            {
                Model = request.Model,
                Provider = request.Provider,
                Scene = request.Scene,
                Messages = request.Messages
                    .Select(m => new ModelMessage { Role = m.Role, Content = m.Content, ToolCallId = m.ToolCallId })
                    .ToList(),
                ToolDefinitions = request.ToolDefinitions,
                Metadata = request.Metadata,
            };

            // 调用 ModelInvokeService
            var response = await modelInvokeService.ChatAsync(applicationId, modelRequest, cancellationToken);

            // 将 ModelMod DTO 转换回 CoreMod DTO
            return new ModelInvokeResponse
            {
                Success = response.Success,
                Content = response.Content,
                ToolCalls = response.ToolCalls,
                Usage = new UsageStats
                {
                    PromptTokens = response.Usage.PromptTokens,
                    CompletionTokens = response.Usage.CompletionTokens,
                    TotalTokens = response.Usage.TotalTokens,
                },
                ErrorMessage = response.ErrorMessage,
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Model invocation failed for application {ApplicationId}", applicationId);
            return new ModelInvokeResponse
            {
                Success = false,
                ErrorMessage = ex.Message,
            };
        }
    }
}
