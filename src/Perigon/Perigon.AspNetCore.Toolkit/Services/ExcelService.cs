using System.Reflection;
using MiniExcelLibs;

namespace Perigon.AspNetCore.Toolkit.Services;

/// <summary>
/// excel 操作类
/// </summary>
public class ExcelService
{
    public const string MimeType =
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    public ExcelService() { }

    /// <summary>
    /// 快捷导出
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data"></param>
    /// <param name="sheetName"></param>
    /// <param name="hasTitle">是否包含标题</param>
    /// <returns></returns>
    public static async Task<Stream> ExportAsync<T>(
        IEnumerable<T> data,
        string sheetName = "sheet1",
        bool hasTitle = true
    )
    {
        var stream = new MemoryStream();
        await stream.SaveAsAsync(data, printHeader: hasTitle, sheetName: sheetName);
        stream.Position = 0;
        return stream;
    }

    /// <summary>
    /// 快捷导入
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="stream"></param>
    /// <param name="sheetName"></param>
    /// <param name="hasTitle">是否包含标题</param>
    /// <returns></returns>
    public static List<T> Import<T>(Stream stream, string? sheetName = null, bool hasTitle = true)
    {
        stream.Position = 0;
        var rows = MiniExcel.Query(stream, useHeaderRow: hasTitle, sheetName: sheetName)
            .Cast<IDictionary<string, object>>();
        return MapRows<T>(rows, hasTitle);
    }

    private static List<T> MapRows<T>(IEnumerable<IDictionary<string, object>> rows, bool hasTitle)
    {
        var result = new List<T>();
        var targetType = typeof(T);
        var properties = targetType
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(property => property.CanWrite)
            .ToArray();

        var propertyMap = properties.ToDictionary(
            property => property.Name,
            property => property,
            StringComparer.OrdinalIgnoreCase
        );

        foreach (var row in rows)
        {
            var instance = Activator.CreateInstance<T>();

            if (hasTitle)
            {
                foreach (var (columnName, columnValue) in row)
                {
                    if (!propertyMap.TryGetValue(columnName, out var property))
                    {
                        continue;
                    }

                    SetPropertyValue(instance, property, columnValue);
                }
            }
            else
            {
                var values = row.Values.ToArray();
                var maxIndex = Math.Min(values.Length, properties.Length);
                for (var index = 0; index < maxIndex; index++)
                {
                    SetPropertyValue(instance, properties[index], values[index]);
                }
            }

            result.Add(instance);
        }

        return result;
    }

    private static void SetPropertyValue<T>(T instance, PropertyInfo property, object? rawValue)
    {
        if (rawValue is null or DBNull)
        {
            return;
        }

        var targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
        var value = rawValue;

        if (targetType.IsEnum)
        {
            value = rawValue is string enumText
                ? Enum.Parse(targetType, enumText, ignoreCase: true)
                : Enum.ToObject(targetType, rawValue);
        }
        else if (targetType != rawValue.GetType())
        {
            value = Convert.ChangeType(rawValue, targetType);
        }

        property.SetValue(instance, value);
    }
}
