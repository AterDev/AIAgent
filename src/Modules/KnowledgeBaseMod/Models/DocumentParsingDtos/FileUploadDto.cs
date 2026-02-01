namespace KnowledgeBaseMod.Models.DocumentParsingDtos;

/// <summary>
/// 文档解析请求
/// </summary>
public class DocumentParseRequestDto
{
    /// <summary>
    /// 文件路径（上传后返回）
    /// </summary>
    public required string FilePath { get; set; }

    /// <summary>
    /// 文件名
    /// </summary>
    public required string FileName { get; set; }
}
