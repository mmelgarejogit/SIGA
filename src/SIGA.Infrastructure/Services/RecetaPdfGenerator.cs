using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SIGA.Application.DTOs.Clinica;
using SIGA.Application.DTOs.Configuracion;
using SIGA.Application.Interfaces;

namespace SIGA.Infrastructure.Services;

public class RecetaPdfGenerator : IRecetaPdfGenerator
{
    private static readonly string PrimaryColor  = "#00288E";
    private static readonly string OutlineColor  = "#757684";
    private static readonly string BorderColor   = "#E0E2E7";

    public byte[] Generate(ConsultaClinicaResponse consulta, ConfiguracionNegocioResponse? config = null)
    {
        var receta = consulta.Receta!;

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A5);
                page.Margin(1.5f, Unit.Centimetre);
                page.DefaultTextStyle(t => t.FontFamily("Arial").FontSize(9).FontColor("#181C20"));

                page.Content().Column(col =>
                {
                    col.Spacing(14);

                    // ── Encabezado ──────────────────────────────────────────
                    col.Item()
                        .BorderBottom(1.5f).BorderColor(PrimaryColor)
                        .PaddingBottom(10)
                        .Column(h =>
                        {
                            h.Item().Text("RECETA ÓPTICA")
                                .FontSize(20).Bold().FontColor(PrimaryColor);
                            if (!string.IsNullOrWhiteSpace(config?.NombreFantasia))
                                h.Item().PaddingTop(1).Text(config.NombreFantasia)
                                    .FontSize(9).FontColor(OutlineColor);
                            h.Item().PaddingTop(2).Text(
                                $"Fecha de emisión: {FormatDate(receta.FechaEmision.ToDateTime(TimeOnly.MinValue))}")
                                .FontSize(8.5f).FontColor(OutlineColor);
                        });

                    // ── Paciente + Profesional ───────────────────────────────
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("PACIENTE")
                                .FontSize(7).Bold().FontColor(OutlineColor)
                                .LetterSpacing(0.08f);
                            c.Item().PaddingTop(3).Text(
                                $"{consulta.PatientFirstName} {consulta.PatientLastName}")
                                .FontSize(10).Bold();
                            c.Item().Text($"CI {consulta.PatientCI}")
                                .FontSize(8.5f).FontColor(OutlineColor);
                        });

                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("PROFESIONAL")
                                .FontSize(7).Bold().FontColor(OutlineColor)
                                .LetterSpacing(0.08f);
                            c.Item().PaddingTop(3).Text(
                                $"{consulta.ProfessionalFirstName} {consulta.ProfessionalLastName}")
                                .FontSize(10).Bold();
                        });
                    });

                    // ── Sección prescripción ─────────────────────────────────
                    col.Item().Column(c =>
                    {
                        c.Item().PaddingBottom(6).Text("PRESCRIPCIÓN ÓPTICA")
                            .FontSize(7).Bold().FontColor(OutlineColor)
                            .LetterSpacing(0.08f);

                        c.Item().Table(table =>
                        {
                            table.ColumnsDefinition(cols =>
                            {
                                cols.ConstantColumn(36);
                                cols.RelativeColumn();
                                cols.RelativeColumn();
                                cols.RelativeColumn();
                                cols.RelativeColumn();
                            });

                            // Encabezado tabla
                            HeaderCell(table, "");
                            HeaderCell(table, "Esfera");
                            HeaderCell(table, "Cilindro");
                            HeaderCell(table, "Eje");
                            HeaderCell(table, "Adición");

                            // OD
                            EyeLabel(table, "OD");
                            DataCell(table, FormatDioptria(receta.OdEsferico));
                            DataCell(table, FormatDioptria(receta.OdCilindro));
                            DataCell(table, FormatEje(receta.OdEje));
                            DataCell(table, FormatDioptria(receta.OdAdicion));

                            // OI
                            EyeLabel(table, "OI");
                            DataCell(table, FormatDioptria(receta.OiEsferico));
                            DataCell(table, FormatDioptria(receta.OiCilindro));
                            DataCell(table, FormatEje(receta.OiEje));
                            DataCell(table, FormatDioptria(receta.OiAdicion));
                        });
                    });

                    // ── Datos adicionales ────────────────────────────────────
                    col.Item().Column(c =>
                    {
                        c.Spacing(4);

                        if (receta.DistanciaInterpupilar.HasValue)
                            c.Item().Row(r =>
                            {
                                r.ConstantItem(130).Text("Distancia Interpupilar:")
                                    .FontSize(8.5f).FontColor(OutlineColor);
                                r.RelativeItem().Text($"{receta.DistanciaInterpupilar:F1} mm")
                                    .FontSize(8.5f).Bold();
                            });

                        if (!string.IsNullOrWhiteSpace(receta.AvSinCorreccion))
                            c.Item().Row(r =>
                            {
                                r.ConstantItem(130).Text("AV sin corrección:")
                                    .FontSize(8.5f).FontColor(OutlineColor);
                                r.RelativeItem().Text(receta.AvSinCorreccion)
                                    .FontSize(8.5f).Bold();
                            });

                        if (!string.IsNullOrWhiteSpace(receta.AvConCorreccion))
                            c.Item().Row(r =>
                            {
                                r.ConstantItem(130).Text("AV con corrección:")
                                    .FontSize(8.5f).FontColor(OutlineColor);
                                r.RelativeItem().Text(receta.AvConCorreccion)
                                    .FontSize(8.5f).Bold();
                            });
                    });

                    // ── Observaciones ────────────────────────────────────────
                    if (!string.IsNullOrWhiteSpace(receta.Observaciones))
                    {
                        col.Item().Column(c =>
                        {
                            c.Item().Text("OBSERVACIONES")
                                .FontSize(7).Bold().FontColor(OutlineColor)
                                .LetterSpacing(0.08f);
                            c.Item().PaddingTop(4)
                                .Border(1).BorderColor(BorderColor)
                                .Padding(8)
                                .Text(receta.Observaciones).FontSize(8.5f);
                        });
                    }

                    // ── Firma ────────────────────────────────────────────────
                    col.Item().PaddingTop(8).AlignRight().Column(c =>
                    {
                        c.Item().AlignRight().Width(140)
                            .BorderBottom(1).BorderColor("#181C20")
                            .PaddingBottom(4)
                            .Text("").FontSize(9);
                        c.Item().AlignRight().PaddingTop(4)
                            .Text($"{consulta.ProfessionalFirstName} {consulta.ProfessionalLastName}")
                            .FontSize(8.5f).FontColor(OutlineColor);
                    });
                });
            });
        })
        .GeneratePdf();
    }

    // ── Helpers de tabla ──────────────────────────────────────────────────────

    private static void HeaderCell(TableDescriptor table, string text)
    {
        table.Cell()
            .Background("#F0F3FA")
            .Border(1).BorderColor(BorderColor)
            .Padding(5)
            .AlignCenter()
            .Text(text).FontSize(7.5f).Bold().FontColor(OutlineColor);
    }

    private static void EyeLabel(TableDescriptor table, string label)
    {
        table.Cell()
            .Background("#F0F3FA")
            .Border(1).BorderColor(BorderColor)
            .Padding(5)
            .AlignCenter()
            .Text(label).FontSize(9).Bold().FontColor(PrimaryColor);
    }

    private static void DataCell(TableDescriptor table, string value)
    {
        table.Cell()
            .Border(1).BorderColor(BorderColor)
            .Padding(5)
            .AlignCenter()
            .Text(value).FontSize(9);
    }

    // ── Helpers de formato ────────────────────────────────────────────────────

    private static string FormatDioptria(decimal? value)
    {
        if (!value.HasValue) return "—";
        return value >= 0 ? $"+{value:F2}" : $"{value:F2}";
    }

    private static string FormatEje(decimal? value)
    {
        if (!value.HasValue) return "—";
        return $"{(int)value}°";
    }

    private static string FormatDate(DateTime date)
    {
        return date.ToString("dd 'de' MMMM 'de' yyyy",
            new System.Globalization.CultureInfo("es-AR"));
    }
}
