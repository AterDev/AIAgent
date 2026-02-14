using CoreMod.Models.ModelInvoke;

namespace CoreMod.Abstraction;

/// <summary>
/// AI 模型调用接口 - CoreMod 定义，由 ModelMod 实现
/// </summary>
public interface IModelInvoker
{
    /// <summary>
    /// 调用 AI 模型进行对话
    /// </summary>
    /// <param name="applicationId">应用 ID</param>
    /// <param name="request">模型调用请求</param>
    /// <param name="cancellationToken"></param>
    /// <returns>模型响应</returns>
    Task<ModelInvokeResponse> ChatAsync(Guid applicationId, ModelInvokeRequest request, CancellationToken cancellationToken = default);
}