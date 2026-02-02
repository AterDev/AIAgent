namespace ModelMod.Models.ModelDebugDtos;

/// <summary>
/// 模型调试请求
/// </summary>
public class ModelDebugRequestDto
{
    /// <summary>
    /// 模型ID
    /// </summary>
    public required string ModelId { get; set; }

    /// <summary>
    /// 用户提示词
    /// </summary>
    public required string Prompt { get; set; }

    /// <summary>
    /// 系统提示词
    /// </summary>
    public string? SystemPrompt { get; set; }

    /// <summary>
    /// 温度参数 (0-2)
    /// </summary>
    public double? Temperature { get; set; }

    /// <summary>
    /// 最大生成Token数
    /// </summary>
    public int? MaxTokens { get; set; }
}
