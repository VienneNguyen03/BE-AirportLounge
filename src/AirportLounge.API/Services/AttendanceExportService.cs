using AirportLounge.Application.Features.Attendance.Queries;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AirportLounge.API.Services;

public static class AttendanceExportService
{
    public static string ToCsv(List<AttendanceReportDto> items)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Id,EmployeeCode,EmployeeName,Date,ShiftName,CheckIn,CheckOut,WorkedHours,Status,IsManuallyAdjusted,IsConfirmed");
        foreach (var a in items)
        {
            sb.AppendLine($"{a.Id},{EscapeCsv(a.EmployeeCode)},{EscapeCsv(a.EmployeeName)},{a.Date:yyyy-MM-dd},{EscapeCsv(a.ShiftName)},{a.CheckIn:yyyy-MM-dd HH:mm},{a.CheckOut:yyyy-MM-dd HH:mm},{a.WorkedHours?.ToString("F2") ?? ""},{a.Status},{a.IsManuallyAdjusted},{a.IsConfirmed}");
        }
        return sb.ToString();
    }

    private static string EscapeCsv(string? value) =>
        value is null ? "" : value.Contains(',') || value.Contains('"') ? $"\"{value.Replace("\"", "\"\"")}\"" : value;

    public static byte[] ToPdf(List<AttendanceReportDto> items, DateTime startDate, DateTime endDate)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(2, Unit.Centimetre);
                page.Header().Column(col =>
                {
                    col.Item().AlignCenter().Text("Attendance Report")
                        .Bold().FontSize(18).FontColor(Colors.Blue.Darken2);
                    col.Item().AlignCenter().Text($"{startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}")
                        .FontSize(10).FontColor(Colors.Grey.Darken1);
                });
                page.Content().PaddingVertical(1, Unit.Centimetre).Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.ConstantColumn(80);
                        c.RelativeColumn(3);
                        c.RelativeColumn(3);
                        c.RelativeColumn(2);
                        c.RelativeColumn(2);
                        c.RelativeColumn(2);
                        c.RelativeColumn(2);
                        c.RelativeColumn(1);
                        c.RelativeColumn(1);
                        c.RelativeColumn(1);
                    });
                    table.Header(h =>
                    {
                        h.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("Code").Bold();
                        h.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("Name").Bold();
                        h.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("Date").Bold();
                        h.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("Shift").Bold();
                        h.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("CheckIn").Bold();
                        h.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("CheckOut").Bold();
                        h.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("Hours").Bold();
                        h.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("Status").Bold();
                        h.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("Adj").Bold();
                        h.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("Conf").Bold();
                    });
                    foreach (var a in items)
                    {
                        table.Cell().BorderBottom(0.5f).Padding(4).Text(a.EmployeeCode).FontSize(8);
                        table.Cell().BorderBottom(0.5f).Padding(4).Text(a.EmployeeName).FontSize(8);
                        table.Cell().BorderBottom(0.5f).Padding(4).Text(a.Date.ToString("yyyy-MM-dd")).FontSize(8);
                        table.Cell().BorderBottom(0.5f).Padding(4).Text(a.ShiftName).FontSize(8);
                        table.Cell().BorderBottom(0.5f).Padding(4).Text(a.CheckIn?.ToString("HH:mm") ?? "-").FontSize(8);
                        table.Cell().BorderBottom(0.5f).Padding(4).Text(a.CheckOut?.ToString("HH:mm") ?? "-").FontSize(8);
                        table.Cell().BorderBottom(0.5f).Padding(4).Text(a.WorkedHours?.ToString("F1") ?? "-").FontSize(8);
                        table.Cell().BorderBottom(0.5f).Padding(4).Text(a.Status).FontSize(8);
                        table.Cell().BorderBottom(0.5f).Padding(4).Text(a.IsManuallyAdjusted ? "Y" : "N").FontSize(8);
                        table.Cell().BorderBottom(0.5f).Padding(4).Text(a.IsConfirmed ? "Y" : "N").FontSize(8);
                    }
                });
            });
        });
        return document.GeneratePdf();
    }
}
