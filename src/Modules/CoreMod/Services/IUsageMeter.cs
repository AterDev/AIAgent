using CoreMod.Models;

namespace CoreMod.Services;

/// <summary>
/// 用量统计解析
/// </summary>
public interface IUsageMeter
{
    UsageStats ReadUsage(ModelResponse response);
}
