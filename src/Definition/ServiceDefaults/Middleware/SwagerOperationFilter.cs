using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ServiceDefaults.Middleware;

public class SwagerOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation?.RequestBody?.Content == null || operation.RequestBody.Content.Count == 0)
        {
            return;
        }

        var content = operation.RequestBody.Content;

        // 1. 检查 EndpointMetadata（兼容 Minimal API / Endpoint routing）
        var endpointMetadata = context.ApiDescription.ActionDescriptor?.EndpointMetadata;
        if (endpointMetadata != null)
        {
            // 也检查是否有传统的 ConsumesAttribute 出现在 EndpointMetadata 中
            var consumesAttr = endpointMetadata.OfType<ConsumesAttribute>().FirstOrDefault();
            if (consumesAttr != null && consumesAttr.ContentTypes?.Count > 0)
            {
                var allowed = BuildAllowedMediaTypes(content, consumesAttr.ContentTypes);
                if (allowed.Count > 0)
                {
                    ReplaceContent(content, allowed);
                    return;
                }
            }
        }

        // 2. 检查 MethodInfo / Controller 上的 [Consumes]（传统 MVC 控制器）
        var methodInfo = context.MethodInfo;
        var consumesFromMethod = methodInfo?.GetCustomAttributes(true).OfType<ConsumesAttribute>().FirstOrDefault()
                                 ?? methodInfo?.DeclaringType?.GetCustomAttributes(true).OfType<ConsumesAttribute>().FirstOrDefault();
        if (consumesFromMethod != null && consumesFromMethod.ContentTypes?.Count > 0)
        {
            var allowed = BuildAllowedMediaTypes(content, consumesFromMethod.ContentTypes);
            if (allowed.Count > 0)
            {
                ReplaceContent(content, allowed);
                return;
            }
        }

        // 3. 使用 ApiDescription.SupportedRequestFormats 推断
        var apiDesc = context.ApiDescription;
        var formats = apiDesc?.SupportedRequestFormats
                             .Select(f => f.MediaType?.Trim())
                             .Where(s => !string.IsNullOrWhiteSpace(s))
                             .Select(s => s!)
                             .Distinct(StringComparer.OrdinalIgnoreCase)
                             .ToList();

        if (formats != null && formats.Count > 0)
        {
            // 优先保留 application/json
            if (formats.Any(f => f.Equals("application/json", StringComparison.OrdinalIgnoreCase)))
            {
                if (content.TryGetValue("application/json", out var jsonMt))
                {
                    ReplaceContent(content, new Dictionary<string, OpenApiMediaType>
                    {
                        ["application/json"] = jsonMt
                    });
                    return;
                }
            }

            // 如果明确包含 multipart/form-data 或 text/event-stream 等，优先保留
            var preferred = formats.FirstOrDefault(f =>
                f.Equals("multipart/form-data", StringComparison.OrdinalIgnoreCase)
                || f.Equals("text/event-stream", StringComparison.OrdinalIgnoreCase)
                || f.Contains("+json", StringComparison.OrdinalIgnoreCase)
            );

            if (!string.IsNullOrEmpty(preferred) && content.TryGetValue(preferred, out var prefMt))
            {
                ReplaceContent(content, new Dictionary<string, OpenApiMediaType>
                {
                    [preferred] = prefMt
                });
                return;
            }
        }

        // 4. 基于参数类型推断（IFormFile / FromForm）
        var hasFileParam = methodInfo?.GetParameters()
            .Any(p => typeof(IFormFile).IsAssignableFrom(p.ParameterType)
                   || typeof(IFormFileCollection).IsAssignableFrom(p.ParameterType)
                   || p.GetCustomAttributes(true).OfType<FromFormAttribute>().Any()
                   || p.ParameterType.Name.Contains("FormFile", StringComparison.OrdinalIgnoreCase)) ?? false;

        if (hasFileParam && content.TryGetValue("multipart/form-data", out var multipartMt))
        {
            ReplaceContent(content, new Dictionary<string, OpenApiMediaType>
            {
                ["multipart/form-data"] = multipartMt
            });
            return;
        }

        // 5. 默认：如果存在 application/json，则只保留它；否则尝试保留第一个 +json 或 /json
        if (content.ContainsKey("application/json"))
        {
            ReplaceContent(content, new Dictionary<string, OpenApiMediaType>
            {
                ["application/json"] = content["application/json"]
            });
            return;
        }

        var fallback = content.FirstOrDefault(kvp => kvp.Key.EndsWith("+json", StringComparison.OrdinalIgnoreCase)
                                                 || kvp.Key.EndsWith("/json", StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrEmpty(fallback.Key))
        {
            ReplaceContent(content, new Dictionary<string, OpenApiMediaType>
            {
                [fallback.Key] = fallback.Value
            });
            return;
        }

        // 否则保留原样（或按需清空）
    }

    private static Dictionary<string, OpenApiMediaType> BuildAllowedMediaTypes(
        IDictionary<string, OpenApiMediaType> source,
        IEnumerable<string> contentTypes)
    {
        var allowed = new Dictionary<string, OpenApiMediaType>(StringComparer.OrdinalIgnoreCase);

        foreach (var raw in contentTypes)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                continue;
            }

            var ct = raw.Trim();
            if (source.TryGetValue(ct, out var mt))
            {
                allowed[ct] = mt;
            }
        }

        return allowed;
    }

    private static void ReplaceContent(
        IDictionary<string, OpenApiMediaType> target,
        IDictionary<string, OpenApiMediaType> replacement)
    {
        target.Clear();
        foreach (var pair in replacement)
        {
            target[pair.Key] = pair.Value;
        }
    }
}