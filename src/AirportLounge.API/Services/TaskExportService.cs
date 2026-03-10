using AirportLounge.Application.Features.Tasks.Queries;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AirportLounge.API.Services;

public static class TaskExportService
{
    public static string ToCsv(List<TaskDto> items)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Id,Title,Description,Priority,Status,AssignedToName,Zone,DueDate,CompletedAt,CreatedAt");
        foreach (var t in items)
        {
            sb.AppendLine($"{t.Id},{EscapeCsv(t.Title)},{EscapeCsv(t.Description)},{t.Priority},{t.Status},{EscapeCsv(t.AssignedToName)},{EscapeCsv(t.Zone)},{t.DueDate:yyyy-MM-dd},{t.CompletedAt:yyyy-MM-dd HH:mm},{t.CreatedAt:yyyy-MM-dd HH:mm}");
        }
        return sb.ToString();
    }

    private static string EscapeCsv(string? value) =>
        value is null ? "" : value.Contains(',') || value.Contains('"') ? $"\"{value.Replace("\"", "\"\"")}\"" : value;

    public static byte[] ToPdf(List<TaskDto> items)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(2, Unit.Centimetre);
                page.Header().Text("Tasks Report").Bold().FontSize(18).FontColor(Colors.Blue.Darken2);
                page.Content().PaddingVertical(1, Unit.Centimetre).Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(3);
                        c.RelativeColumn(2);
                        c.RelativeColumn(1);
                        c.RelativeColumn(1);
                        c.RelativeColumn(2);
                        c.RelativeColumn(2);
                        c.RelativeColumn(2);
                    });
                    table.Header(h =>
                    {
                        h.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("Title").Bold();
                        h.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("Priority").Bold();
                        h.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("Status").Bold();
                        h.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("Zone").Bold();
                        h.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("AssignedTo").Bold();
                        h.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("DueDate").Bold();
                        h.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("CreatedAt").Bold();
                    });
                    foreach (var t in items)
                    {
                        table.Cell().BorderBottom(0.5f).Padding(4).Text(t.Title).FontSize(8);
                        table.Cell().BorderBottom(0.5f).Padding(4).Text(t.Priority).FontSize(8);
                        table.Cell().BorderBottom(0.5f).Padding(4).Text(t.Status).FontSize(8);
                        table.Cell().BorderBottom(0.5f).Padding(4).Text(t.Zone ?? "-").FontSize(8);
                        table.Cell().BorderBottom(0.5f).Padding(4).Text(t.AssignedToName ?? "-").FontSize(8);
                        table.Cell().BorderBottom(0.5f).Padding(4).Text(t.DueDate?.ToString("yyyy-MM-dd") ?? "-").FontSize(8);
                        table.Cell().BorderBottom(0.5f).Padding(4).Text(t.CreatedAt.ToString("yyyy-MM-dd HH:mm")).FontSize(8);
                    }
                });
            });
        });
        return document.GeneratePdf();
    }
}
