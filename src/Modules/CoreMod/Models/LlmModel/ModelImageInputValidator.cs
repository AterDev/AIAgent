using Share.Exceptions;

namespace CoreMod.Models;

/// <summary>
/// 多模态图片输入校验器。
/// 约束与前端保持一致：最多 4 张，单张原始大小不超过 5MB。
/// </summary>
public static class ModelImageInputValidator
{
    public const int MaxImageCount = 4;
    public const int MaxImageBytes = 5 * 1024 * 1024;
    public const int MaxRemoteUrlLength = 2000;

    public static List<ModelAttachment> BuildValidatedImageAttachments(IEnumerable<string>? images)
    {
        var values = images?
            .Where(uri => !string.IsNullOrWhiteSpace(uri))
            .Select(uri => uri.Trim())
            .ToList() ?? [];

        if (values.Count > MaxImageCount)
        {
            throw new BusinessException($"Too many images. Maximum allowed is {MaxImageCount}.");
        }

        return values.Select(BuildValidatedImageAttachment).ToList();
    }

    private static ModelAttachment BuildValidatedImageAttachment(string value)
    {
        if (value.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
        {
            return BuildFromDataUri(value);
        }

        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new BusinessException("Only data:image/* base64 URIs or http(s) image URLs are allowed.");
        }

        if (value.Length > MaxRemoteUrlLength)
        {
            throw new BusinessException($"Image URL is too long. Maximum allowed length is {MaxRemoteUrlLength}.");
        }

        return new ModelAttachment
        {
            Kind = "image",
            DataUri = value,
            MediaType = "image/*",
        };
    }

    private static ModelAttachment BuildFromDataUri(string value)
    {
        var commaIndex = value.IndexOf(',');
        if (commaIndex <= 5)
        {
            throw new BusinessException("Invalid data URI image payload.");
        }

        var metadata = value[5..commaIndex];
        if (!metadata.Contains(";base64", StringComparison.OrdinalIgnoreCase))
        {
            throw new BusinessException("Only base64-encoded data URI images are supported.");
        }

        var mediaType = metadata.Split(';', 2)[0];
        if (!mediaType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            throw new BusinessException("Only image/* data URI inputs are supported.");
        }

        var payload = value[(commaIndex + 1)..].Trim();
        if (payload.Length == 0 || payload.Length % 4 != 0)
        {
            throw new BusinessException("Invalid base64 image payload.");
        }

        var padding = payload.EndsWith("==", StringComparison.Ordinal) ? 2
            : payload.EndsWith("=", StringComparison.Ordinal) ? 1
            : 0;
        var estimatedBytes = (payload.Length / 4 * 3) - padding;

        if (estimatedBytes > MaxImageBytes)
        {
            throw new BusinessException($"Image payload is too large. Maximum allowed size is {MaxImageBytes} bytes.");
        }

        return new ModelAttachment
        {
            Kind = "image",
            DataUri = value,
            MediaType = mediaType,
        };
    }
}