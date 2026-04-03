using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Entity.ModelMod;

namespace Entity.AIAgentMod;

/// <summary>
/// 应用侧 Agent
/// </summary>
[Index(nameof(ApplicationId), nameof(Name), IsUnique = true)]
[Index(nameof(ApplicationId))]
[Index(nameof(UserId))]
public class ApplicationAgent : EntityBase
{
    /// <summary>
    /// Agent 名称
    /// </summary>
    [MaxLength(100)]
    public required string Name { get; set; }

    /// <summary>
    /// Agent 描述信息
    /// </summary>
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Agent 所使用的大模型名称（例如 "gpt-4", "qwen-max", "custom-llm"）
    /// </summary>
    public required string ModelId { get; set; }

    /// <summary>
    /// Agent 的角色设定（System Prompt）
    /// </summary>
    [MaxLength(5000)]
    public string SystemPrompt { get; set; } = string.Empty;

    /// <summary>
    /// Agent 可用的工具列表
    /// </summary>
    public List<string> Tools { get; set; } = [];

    /// <summary>
    /// is enabled
    /// </summary>
    public bool Enable { get; set; }

    public Guid ApplicationId { get; set; }

    [ForeignKey(nameof(ApplicationId))]
    public Application Application { get; set; } = null!;

    public Guid? UserId { get; set; }
}