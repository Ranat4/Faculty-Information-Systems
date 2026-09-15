using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace FacultyInformationSystem_FIS_.Services
{
    public class ExportService : IExportService
    {
        public byte[] BuildExcel(string title, IReadOnlyList<string> headers, IReadOnlyList<IReadOnlyList<string>> rows)
        {
            using var workbook = new XLWorkbook();
            var sheetName = title.Length > 31 ? title.Substring(0, 31) : title; // Excel sheet name limit
            var worksheet = workbook.Worksheets.Add(sheetName);

            for (var col = 0; col < headers.Count; col++)
            {
                var cell = worksheet.Cell(1, col + 1);
                cell.Value = headers[col];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#181818");
                cell.Style.Font.FontColor = XLColor.White;
            }

            for (var row = 0; row < rows.Count; row++)
            {
                for (var col = 0; col < rows[row].Count; col++)
                {
                    worksheet.Cell(row + 2, col + 1).Value = rows[row][col];
                }
            }

            worksheet.Columns().AdjustToContents();
            worksheet.SheetView.FreezeRows(1);

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public byte[] BuildPdf(string title, IReadOnlyList<string> headers, IReadOnlyList<IReadOnlyList<string>> rows)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(24);
                    page.DefaultTextStyle(x => x.FontSize(9));

                    page.Header().Column(col =>
                    {
                        col.Item().Text(title).FontSize(16).Bold();
                        col.Item().Text($"Generated {DateTime.Now:dd MMM yyyy HH:mm}").FontSize(8).FontColor(Colors.Grey.Darken1);
                        col.Item().PaddingTop(8).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                    });

                    page.Content().PaddingTop(10).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            foreach (var _ in headers)
                            {
                                columns.RelativeColumn();
                            }
                        });

                        table.Header(header =>
                        {
                            foreach (var h in headers)
                            {
                                header.Cell().Background("#181818").Padding(4)
                                    .Text(h).FontColor(Colors.White).Bold();
                            }
                        });

                        foreach (var row in rows)
                        {
                            foreach (var value in row)
                            {
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(value);
                            }
                        }
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.CurrentPageNumber();
                        x.Span(" / ");
                        x.TotalPages();
                    });
                });
            });

            return document.GeneratePdf();
        }
    }
}
