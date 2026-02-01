namespace WorkflowMod.Models.WorkflowExecutionDtos;

/// <summary>
/// 表格查询参数
/// </summary>
public class TableQueryDto
{
    /// <summary>
    /// 页码（从 1 开始）
    /// </summary>
    [Range(1, int.MaxValue)]
    public int PageIndex { get; set; } = 1;

    /// <summary>
    /// 每页行数
    /// </summary>
    [Range(1, 100)]
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// 关键词搜索
    /// </summary>
    [MaxLength(200)]
    public string? Keyword { get; set; }

    /// <summary>
    /// 排序字段
    /// </summary>
    [MaxLength(50)]
    public string? SortBy { get; set; }

    /// <summary>
    /// 排序顺序（asc/desc）
    /// </summary>
    [RegularExpression("(asc|desc)")]
    public string SortOrder { get; set; } = "asc";

    /// <summary>
    /// 多条件筛选（JSON 格式）
    /// </summary>
    [MaxLength(1000)]
    public string? FiltersJson { get; set; }
}

/// <summary>
/// 表格响应数据
/// </summary>
public class TableResponseDto<T>
{
    public List<T> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int PageIndex { get; set; }
    public int PageSize { get; set; }

    [JsonIgnore]
    public int TotalPages => (TotalCount + PageSize - 1) / PageSize;
}
