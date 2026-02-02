namespace ModelMod.Models.ModelDebugDtos;

/// <summary>
/// 模型调试响应
/// </summary>
public class ModelDebugResponseDto
{
    /// <summary>
    /// 生成的内容
    /// </summary>
    public required string Content { get; set; }

    /// <summary>
    /// 使用的模型
    /// </summary>
    public required string Model { get; set; }

    /// <summary>
    /// 提示词Token数
    /// </summary>
    public int PromptTokens { get; set; }

    /// <summary>
    /// 生成Token数
    /// </summary>
    public int CompletionTokens { get; set; }

    /// <summary>
    /// 总Token数
    /// </summary>
    public int TotalTokens { get; set; }

    /// <summary>
    /// 完成原因
    /// </summary>
    public required string FinishReason { get; set; }

    /// <summary>
    /// 调用耗时(毫秒)
    /// </summary>
    public long Duration { get; set; }

    /// <summary>
    /// 错误信息(如果有)
    /// </summary>
    public string? ErrorMessage { get; set; }
}
