using System.Text;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SIGA.Application.DTOs.Reportes;
using SIGA.Application.Interfaces;

namespace SIGA.Infrastructure.Services;

public class ReporteExporter : IReporteExporter
{
    private const string Primary   = "#00288E";
    private const string Outline   = "#757684";
    private const string Border    = "#E0E2E7";
    private const string RowAlt    = "#F7F9FE";
    private const string TotalsBg  = "#EEF1FA";
    private const string White     = "#FFFFFF";

    public byte[] ToPdf(ReporteExport data)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(1.2f, Unit.Centimetre);
                page.DefaultTextStyle(t => t.FontFamily("Arial").FontSize(8).FontColor("#181C20"));

                page.Header().BorderBottom(1.5f).BorderColor(Primary).PaddingBottom(6).Column(col =>
                {
                    col.Item().Text(data.Titulo).FontSize(15).Bold().FontColor(Primary);
                    if (!string.IsNullOrWhiteSpace(data.Subtitulo))
                        col.Item().Text(data.Subtitulo).FontSize(9).FontColor(Outline);
                });

                page.Content().PaddingTop(8).Table(table =>
                {
                    table.ColumnsDefinition(cd =>
                    {
                        foreach (var _ in data.Columnas) cd.RelativeColumn();
                    });

                    table.Header(header =>
                    {
                        for (int i = 0; i < data.Columnas.Length; i++)
                        {
                            IContainer cell = header.Cell().Background(Primary).PaddingVertical(4).PaddingHorizontal(4);
                            if (data.ColumnasNumericas.Contains(i)) cell = cell.AlignRight();
                            cell.Text(data.Columnas[i]).FontSize(8).Bold().FontColor(White);
                        }
                    });

                    int rowIdx = 0;
                    foreach (var fila in data.Filas)
                    {
                        var bg = rowIdx % 2 == 1 ? RowAlt : White;
                        for (int i = 0; i < data.Columnas.Length; i++)
                        {
                            var val = i < fila.Length ? fila[i] : "";
                            IContainer cell = table.Cell().Background(bg)
                                .BorderBottom(0.5f).BorderColor(Border).PaddingVertical(3).PaddingHorizontal(4);
                            if (data.ColumnasNumericas.Contains(i)) cell = cell.AlignRight();
                            cell.Text(val).FontSize(8);
                        }
                        rowIdx++;
                    }

                    if (data.Totales is { Length: > 0 })
                    {
                        for (int i = 0; i < data.Columnas.Length; i++)
                        {
                            var val = i < data.Totales.Length ? data.Totales[i] : "";
                            IContainer cell = table.Cell().Background(TotalsBg).PaddingVertical(4).PaddingHorizontal(4);
                            if (data.ColumnasNumericas.Contains(i)) cell = cell.AlignRight();
                            cell.Text(val).FontSize(8).Bold();
                        }
                    }
                });

                page.Footer().AlignCenter().Text(t =>
                {
                    t.Span("Generado el ").FontSize(7).FontColor(Outline);
                    t.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm")).FontSize(7).FontColor(Outline);
                    t.Span("   ·   Página ").FontSize(7).FontColor(Outline);
                    t.CurrentPageNumber().FontSize(7).FontColor(Outline);
                    t.Span(" / ").FontSize(7).FontColor(Outline);
                    t.TotalPages().FontSize(7).FontColor(Outline);
                });
            });
        }).GeneratePdf();
    }

    public byte[] ToCsv(ReporteExport data)
    {
        var sb = new StringBuilder();
        sb.Append('﻿'); // BOM → Excel detecta UTF-8
        sb.AppendLine(string.Join(';', data.Columnas.Select(CsvField)));
        foreach (var fila in data.Filas)
            sb.AppendLine(string.Join(';', fila.Select(CsvField)));
        if (data.Totales is { Length: > 0 })
            sb.AppendLine(string.Join(';', data.Totales.Select(CsvField)));
        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private static string CsvField(string? v)
    {
        v ??= "";
        if (v.Contains(';') || v.Contains('"') || v.Contains('\n') || v.Contains('\r'))
            return "\"" + v.Replace("\"", "\"\"") + "\"";
        return v;
    }
}
