namespace CoreMod.Models;

/// <summary>
/// 模型消息附件（多模态输入，目前主要用于图片）。
/// DataUri 支持 data:image/png;base64,... 或 https/http URL。
/// </summary>
public sealed class ModelAttachment
{
    /// <summary>
    /// 附件类型：image、file 等。默认 image。
    /// </summary>
    public string Kind { get; set; } = "image";

    /// <summary>
    /// data URI 或远程 URL。
    /// </summary>
    public string DataUri { get; set; } = string.Empty;

    /// <summary>
    /// 媒体类型，如 image/png、image/jpeg。
    /// </summary>
    public string? MediaType { get; set; }
}
