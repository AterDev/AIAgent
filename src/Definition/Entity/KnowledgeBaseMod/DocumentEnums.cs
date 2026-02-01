namespace Entity.KnowledgeBaseMod;

/// <summary>
/// 文档格式类型
/// </summary>
public enum DocumentFormatType
{
    /// <summary>
    /// 纯文本
    /// </summary>
    Text = 0,

    /// <summary>
    /// Markdown
    /// </summary>
    Markdown = 1,

    /// <summary>
    /// PDF
    /// </summary>
    Pdf = 2,

    /// <summary>
    /// Word 文档
    /// </summary>
    Word = 3,

    /// <summary>
    /// Excel 电子表格
    /// </summary>
    Excel = 4,

    /// <summary>
    /// PowerPoint 演示文稿
    /// </summary>
    PowerPoint = 5,

    /// <summary>
    /// JSON
    /// </summary>
    Json = 6,

    /// <summary>
    /// XML
    /// </summary>
    Xml = 7
}

/// <summary>
/// 文档解析状态
/// </summary>
public enum DocumentParsingStatus
{
    /// <summary>
    /// 待解析
    /// </summary>
    Pending = 0,

    /// <summary>
    /// 解析中
    /// </summary>
    Parsing = 1,

    /// <summary>
    /// 解析成功
    /// </summary>
    Success = 2,

    /// <summary>
    /// 解析失败
    /// </summary>
    Failed = 3,

    /// <summary>
    /// 已取消
    /// </summary>
    Cancelled = 4
}
