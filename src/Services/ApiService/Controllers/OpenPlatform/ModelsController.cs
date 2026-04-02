using CoreMod.Models;
using ModelMod.Services;
using Share.Exceptions;

namespace ApiService.Controllers.OpenPlatform;

/// <summary>
/// Open platform models
/// </summary>
[ApiController]
[Route("api/v1/models")]
public class ModelsController(
    ModelInvokeService modelInvokeService,
    IUserContext user,
    ILogger<ModelsController> logger
) : OpenApiControllerBase<ModelInvokeService>(modelInvokeService, user, logger)
{
    /// <summary>
    /// Call model directly.
    /// </summary>
    [HttpPost("chat")]
    public async Task<ActionResult<ModelResponse>> ChatAsync(ModelChatRequestDto dto, CancellationToken cancellationToken)
    {
        var applicationId = _user.IsRole(WebConst.Application)
            ? _user.UserId
            : dto.ApplicationId;

        if (!applicationId.HasValue || applicationId == Guid.Empty)
        {
            throw new BusinessException("Application is required");
        }

        if (string.IsNullOrWhiteSpace(dto.Model))
        {
            throw new BusinessException("Model is required");
        }

        if (dto.Messages.Count == 0)
        {
            throw new BusinessException("At least one message is required");
        }

        var metadata = new Dictionary<string, string>();
        if (dto.Temperature.HasValue)
        {
            metadata["temperature"] = dto.Temperature.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        if (dto.MaxTokens.HasValue)
        {
            metadata["max_tokens"] = dto.MaxTokens.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        var request = new ModelRequest
        {
            Model = dto.Model,
            Provider = dto.Provider,
            Scene = dto.Scene,
            Messages = dto.Messages,
            Metadata = metadata,
        };

        var response = await _manager.ChatAsync(applicationId.Value, request, cancellationToken);
        return Ok(response);
    }
}