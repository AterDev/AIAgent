using ModelMod.Models.ModelDebugDtos;
using CoreMod.Services;
using CoreMod.Models;
using System.Diagnostics;
using System.Globalization;

namespace AdminService.Controllers.ModelMod;

/// <summary>
/// 模型调试工具
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ModelDebugController(
    IModelClient modelClient,
    ILogger<ModelDebugController> logger
) : ControllerBase
{
    private readonly IModelClient _modelClient = modelClient;
    private readonly ILogger<ModelDebugController> _logger = logger;

    /// <summary>
    /// 调试模型调用
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ModelDebugResponseDto>> DebugAsync(ModelDebugRequestDto request)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation("Starting model debug for model: {ModelId}", request.ModelId);

            // Build messages
            var messages = new List<ModelMessage>();
            
            if (!string.IsNullOrWhiteSpace(request.SystemPrompt))
            {
                messages.Add(new ModelMessage
                {
                    Role = "system",
                    Content = request.SystemPrompt
                });
            }

            messages.Add(new ModelMessage
            {
                Role = "user",
                Content = request.Prompt
            });

            // Create model request
            var modelRequest = new ModelRequest
            {
                Model = request.ModelId,
                Messages = messages,
                Metadata = new Dictionary<string, string>()
            };

            // Add optional parameters to metadata
            if (request.Temperature.HasValue)
            {
                modelRequest.Metadata["temperature"] = request.Temperature.Value.ToString(CultureInfo.InvariantCulture);
            }

            if (request.MaxTokens.HasValue)
            {
                modelRequest.Metadata["max_tokens"] = request.MaxTokens.Value.ToString(CultureInfo.InvariantCulture);
            }

            // Call the model
            var response = await _modelClient.ChatAsync(modelRequest);

            stopwatch.Stop();

            // Check if the request was successful
            if (!response.Success)
            {
                _logger.LogWarning(
                    "Model debug failed for model: {ModelId}, Error: {ErrorMessage}",
                    request.ModelId,
                    response.ErrorMessage
                );

                var errorResponseDto = new ModelDebugResponseDto
                {
                    Content = string.Empty,
                    Model = request.ModelId,
                    PromptTokens = 0,
                    CompletionTokens = 0,
                    TotalTokens = 0,
                    FinishReason = "error",
                    Duration = stopwatch.ElapsedMilliseconds,
                    ErrorMessage = response.ErrorMessage
                };

                return StatusCode(500, errorResponseDto);
            }

            // Build success response DTO
            var responseDto = new ModelDebugResponseDto
            {
                Content = response.Content ?? string.Empty,
                Model = request.ModelId,
                PromptTokens = response.Usage?.PromptTokens ?? 0,
                CompletionTokens = response.Usage?.CompletionTokens ?? 0,
                TotalTokens = response.Usage?.TotalTokens ?? 0,
                FinishReason = "stop",
                Duration = stopwatch.ElapsedMilliseconds,
                ErrorMessage = null
            };

            _logger.LogInformation(
                "Model debug completed. Model: {ModelId}, Duration: {Duration}ms, Tokens: {TotalTokens}",
                request.ModelId,
                stopwatch.ElapsedMilliseconds,
                responseDto.TotalTokens
            );

            return Ok(responseDto);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Error during model debug for model: {ModelId}", request.ModelId);

            var errorResponseDto = new ModelDebugResponseDto
            {
                Content = string.Empty,
                Model = request.ModelId,
                PromptTokens = 0,
                CompletionTokens = 0,
                TotalTokens = 0,
                FinishReason = "error",
                Duration = stopwatch.ElapsedMilliseconds,
                ErrorMessage = ex.Message
            };

            return StatusCode(500, errorResponseDto);
        }
    }
}
