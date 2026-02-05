namespace CoreMod.Services;

/// <summary>
/// 系统配置服务接口 - CoreMod 定义，由 SystemMod 实现
/// </summary>
public interface ISystemConfigService
{
    /// <summary>
    /// 获取配置值
    /// </summary>
    /// <param name="category">分类</param>
    /// <param name="key">键</param>
    /// <param name="cancellationToken"></param>
    /// <returns>配置值</returns>
    Task<string?> GetValueAsync(string category, string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// 使用给定数据渲染模板
    /// </summary>
    /// <param name="template">模板文本</param>
    /// <param name="data">数据字典</param>
    /// <returns>渲染后的文本</returns>
    string RenderTemplate(string template, Dictionary<string, string> data);
}
