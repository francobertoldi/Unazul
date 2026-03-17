using ClosedXML.Excel;
using System.Reflection;
using System.Text;

namespace Shared.Export;

public sealed record ColumnDefinition<T>(string Header, Func<T, object?> ValueSelector);

public static class ExportService
{
    private const int MaxRows = 10_000;

    public static byte[] ToXlsx<T>(IEnumerable<T> data, IReadOnlyList<ColumnDefinition<T>> columns, string sheetName = "Export")
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add(sheetName);

        // Headers
        for (var col = 0; col < columns.Count; col++)
        {
            var cell = worksheet.Cell(1, col + 1);
            cell.Value = columns[col].Header;
            cell.Style.Font.Bold = true;
        }

        // Data rows
        var row = 2;
        foreach (var item in data.Take(MaxRows))
        {
            for (var col = 0; col < columns.Count; col++)
            {
                var value = columns[col].ValueSelector(item);
                var cell = worksheet.Cell(row, col + 1);

                switch (value)
                {
                    case null:
                        break;
                    case DateTime dt:
                        cell.Value = dt;
                        cell.Style.DateFormat.Format = "yyyy-MM-dd HH:mm:ss";
                        break;
                    case DateTimeOffset dto:
                        cell.Value = dto.LocalDateTime;
                        cell.Style.DateFormat.Format = "yyyy-MM-dd HH:mm:ss";
                        break;
                    case DateOnly d:
                        cell.Value = d.ToDateTime(TimeOnly.MinValue);
                        cell.Style.DateFormat.Format = "yyyy-MM-dd";
                        break;
                    case decimal dec:
                        cell.Value = dec;
                        break;
                    case double dbl:
                        cell.Value = dbl;
                        break;
                    case int i:
                        cell.Value = i;
                        break;
                    case long l:
                        cell.Value = l;
                        break;
                    case bool b:
                        cell.Value = b ? "Si" : "No";
                        break;
                    default:
                        cell.Value = value.ToString();
                        break;
                }
            }
            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public static byte[] ToCsv<T>(IEnumerable<T> data, IReadOnlyList<ColumnDefinition<T>> columns, string separator = ",")
    {
        var sb = new StringBuilder();

        // Header
        sb.AppendLine(string.Join(separator, columns.Select(c => EscapeCsv(c.Header, separator))));

        // Data rows
        foreach (var item in data.Take(MaxRows))
        {
            var values = columns.Select(c =>
            {
                var value = c.ValueSelector(item);
                return EscapeCsv(value?.ToString() ?? string.Empty, separator);
            });
            sb.AppendLine(string.Join(separator, values));
        }

        return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
    }

    private static string EscapeCsv(string value, string separator)
    {
        if (value.Contains(separator, StringComparison.Ordinal)
            || value.Contains('"')
            || value.Contains('\n')
            || value.Contains('\r'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }
}
