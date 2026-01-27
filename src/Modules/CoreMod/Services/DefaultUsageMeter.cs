using CoreMod.Models;

namespace CoreMod.Services;

/// <summary>
/// 默认用量解析（占位）
/// </summary>
public class DefaultUsageMeter : IUsageMeter
{
    public UsageStats ReadUsage(ModelResponse response)
    {
        return response.Usage;
    }
}
