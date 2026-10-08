namespace Perigon.AspNetCore.Models;

/// <summary>
/// 过滤
/// </summary>
public class FilterBase
{
    public int PageIndex
    {
        get { return field; }
        set { field = value < 1 ? 1 : value; }
    } = 1;

    /// <summary>
    /// max 10000
    /// </summary>
    public int PageSize
    {
        get { return field; }
        set { field = value > 10000 ? 10000 : value < 0 ? 0 : value; }
    } = 20;

    /// <summary>
    /// 排序,field=>是否正序
    /// </summary>
    public Dictionary<string, bool>? OrderBy { get; set; }
}
