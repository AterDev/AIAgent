using MiniExcelLibs;
using MiniExcelLibs.OpenXml;

namespace Perigon.AspNetCore.Toolkit.Services;

/// <summary>
/// excel 操作类
/// </summary>
public class ExcelService
{
    public const int ExcelMaxRows = 1_048_576;
    public const int DefaultExportRowsLimit = ExcelMaxRows - 1;
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
        bool hasTitle = true,
        int rowLimit = DefaultExportRowsLimit
    )
    {
        var stream = new MemoryStream();
        await ExportAsync(stream, data, sheetName, hasTitle, rowLimit);
        stream.Position = 0;
        return stream;
    }

    /// <summary>
    /// 流式导出到目标流。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="stream"></param>
    /// <param name="data"></param>
    /// <param name="sheetName"></param>
    /// <param name="hasTitle">是否包含标题</param>
    /// <param name="rowLimit">数据行上限，不包含标题行</param>
    /// <returns></returns>
    public static async Task ExportAsync<T>(
        Stream stream,
        IEnumerable<T> data,
        string sheetName = "sheet1",
        bool hasTitle = true,
        int rowLimit = DefaultExportRowsLimit
    )
    {
        ValidateRowLimit(rowLimit, hasTitle);
        var configuration = new OpenXmlConfiguration
        {
            FastMode = true,
            EnableAutoWidth = true,
            AutoFilter = false,
            FreezeRowCount = hasTitle ? 1 : 0,
        };

        await stream.SaveAsAsync(
            EnsureRowLimit(data, rowLimit),
            printHeader: hasTitle,
            sheetName: sheetName,
            configuration: configuration
        );
    }

    /// <summary>
    /// 快捷导入
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="stream"></param>
    /// <param name="sheetName"></param>
    /// <param name="hasTitle">是否包含标题</param>
    /// <returns></returns>
    public static List<T> Import<T>(
        Stream stream,
        string? sheetName = null,
        bool hasTitle = true,
        int rowLimit = DefaultExportRowsLimit
    )
        where T : class, new()
    {
        ValidateRowLimit(rowLimit, hasTitle);
        stream.Position = 0;
        var rows = stream
            .Query<T>(sheetName: sheetName, hasHeader: hasTitle)
            .Take(rowLimit + 1)
            .ToList();
        if (rows.Count > rowLimit)
        {
            throw new InvalidOperationException($"Excel import row count exceeds {rowLimit}.");
        }
        return rows;
    }

    private static IEnumerable<T> EnsureRowLimit<T>(IEnumerable<T> data, int rowLimit)
    {
        if (data.TryGetNonEnumeratedCount(out var count) && count > rowLimit)
        {
            throw new InvalidOperationException($"Excel export row count exceeds {rowLimit}.");
        }

        var index = 0;
        foreach (var item in data)
        {
            if (index >= rowLimit)
            {
                throw new InvalidOperationException($"Excel export row count exceeds {rowLimit}.");
            }

            index++;
            yield return item;
        }
    }

    private static void ValidateRowLimit(int rowLimit, bool hasTitle)
    {
        var maxDataRows = hasTitle ? ExcelMaxRows - 1 : ExcelMaxRows;
        if (rowLimit < 1 || rowLimit > maxDataRows)
        {
            throw new ArgumentOutOfRangeException(
                nameof(rowLimit),
                $"Row limit must be between 1 and {maxDataRows}."
            );
        }
    }
}
