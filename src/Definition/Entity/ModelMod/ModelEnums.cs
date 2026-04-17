namespace Entity.ModelMod;

using System.ComponentModel;

public enum ModelProviderType
{
    [Description("OpenAI compatible")]
    OpenAiCompatible = 0,

    [Description("Custom provider")]
    Custom = 1,

    /// <summary>
    /// Microsoft Foundry Local（本地 OpenAI 兼容 endpoint，动态端口）
    /// </summary>
    [Description("Foundry Local")]
    FoundryLocal = 2,

    /// <summary>
    /// Azure OpenAI（需走 Azure endpoint/api-version 协议）
    /// </summary>
    [Description("Azure OpenAI")]
    AzureOpenAI = 3,

    /// <summary>
    /// Anthropic 原生协议
    /// </summary>
    [Description("Anthropic")]
    Anthropic = 4,

    /// <summary>
    /// Google Gemini 原生协议
    /// </summary>
    [Description("Google")]
    Google = 5,
}

public enum InvocationStatus
{
    [Description("Success")]
    Success = 0,

    [Description("Failed")]
    Failed = 1,

    [Description("Timeout")]
    Timeout = 2,

    [Description("Canceled")]
    Canceled = 3,
}

public enum QuotaPeriodType
{
    [Description("Minute")]
    Minute = 0,

    [Description("Hour")]
    Hour = 1,

    [Description("Day")]
    Day = 2,

    [Description("Month")]
    Month = 3,
}
