using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SIGA.Application.DTOs.Inventario;
using SIGA.Application.Interfaces;

namespace SIGA.Infrastructure.Services;

public class MovimientoStockPdfGenerator : IMovimientoStockPdfGenerator
{
    private static readonly string Primary     = "#00288E";
    private static readonly string Outline     = "#757684";
    private static readonly string Border      = "#E0E2E7";
    private static readonly string GreenBg     = "#DCFCE7";
    private static readonly string GreenText   = "#166534";
    private static readonly string RedBg       = "#FEE2E2";
    private static readonly string RedText     = "#991B1B";
    private static readonly string AmberBg     = "#FEF3C7";
    private static readonly string AmberText   = "#92400E";

    public byte[] Generate(MovimientoStockResponse m)
    {
        var (estadoBg, estadoText) = m.Estado switch
        {
            "Aprobado"  => (GreenBg,  GreenText),
            "Rechazado" => (RedBg,    RedText),
            _           => (AmberBg,  AmberText),
        };

        var tipoBg   = m.Tipo == "Entrada" ? GreenBg  : RedBg;
        var tipoText = m.Tipo == "Entrada" ? GreenText : RedText;

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A5.Landscape());
                page.Margin(1.5f, Unit.Centimetre);
                page.DefaultTextStyle(t => t.FontFamily("Arial").FontSize(9).FontColor("#181C20"));

                page.Content().Column(col =>
                {
                    col.Spacing(12);

                    // ── Encabezado ──────────────────────────────────────────
                    col.Item()
                       .BorderBottom(1.5f).BorderColor(Primary)
                       .PaddingBottom(8)
                       .Row(row =>
                       {
                           row.RelativeItem().Column(h =>
                           {
                               h.Item().Text("COMPROBANTE DE MOVIMIENTO DE STOCK")
                                   .FontSize(14).Bold().FontColor(Primary);
                               h.Item().Text($"Nro. {m.Id:D6}")
                                   .FontSize(10).FontColor(Outline);
                           });
                           row.ConstantItem(120).AlignRight().Column(r =>
                           {
                               r.Item().Text(m.FechaMovimiento.ToLocalTime().ToString("dd/MM/yyyy"))
                                   .FontSize(11).Bold();
                               r.Item().Text(m.FechaMovimiento.ToLocalTime().ToString("HH:mm"))
                                   .FontSize(9).FontColor(Outline);
                           });
                       });

                    // ── Badges tipo + estado ───────────────────────────────
                    col.Item().Row(row =>
                    {
                        row.AutoItem()
                           .Background(tipoBg).PaddingVertical(4).PaddingHorizontal(8)
                           .Text(m.Tipo).Bold().FontSize(9).FontColor(tipoText);
                        row.ConstantItem(8);
                        row.AutoItem()
                           .Background(estadoBg).PaddingVertical(4).PaddingHorizontal(8)
                           .Text(m.Estado).Bold().FontSize(9).FontColor(estadoText);
                    });

                    // ── Datos principales ──────────────────────────────────
                    col.Item()
                       .Border(1).BorderColor(Border)
                       .Column(card =>
                       {
                           DataRow(card, "Producto",  m.ProductoNombre);
                           DataRow(card, "Cantidad",  m.Cantidad.ToString());
                           DataRow(card, "Motivo",    m.Motivo ?? "—");
                           DataRow(card, "Registrado por", m.CreadoPorNombre ?? "—");
                       });

                    // ── Aprobación ─────────────────────────────────────────
                    if (m.Estado != "Pendiente")
                    {
                        col.Item()
                           .Border(1).BorderColor(Border)
                           .Column(card =>
                           {
                               var fechaApr = m.FechaAprobacion?.ToLocalTime().ToString("dd/MM/yyyy HH:mm") ?? "—";
                               DataRow(card, m.Estado == "Aprobado" ? "Aprobado por" : "Rechazado por",
                                   m.AprobadoPorNombre ?? "—");
                               DataRow(card, "Fecha",       fechaApr);
                               if (!string.IsNullOrWhiteSpace(m.ObservacionesAprobacion))
                                   DataRow(card, "Observaciones", m.ObservacionesAprobacion);
                           });
                    }
                });

                // Pie de página
                page.Footer()
                    .AlignCenter()
                    .Text(t =>
                    {
                        t.Span("Generado el ").FontSize(7).FontColor(Outline);
                        t.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm")).FontSize(7).FontColor(Outline);
                    });
            });
        }).GeneratePdf();
    }

    private static void DataRow(ColumnDescriptor col, string label, string value)
    {
        col.Item()
           .BorderBottom(1).BorderColor(Border)
           .PaddingVertical(6).PaddingHorizontal(10)
           .Row(row =>
           {
               row.ConstantItem(130)
                  .Text(label.ToUpper())
                  .FontSize(7).Bold().FontColor(Outline);
               row.RelativeItem()
                  .Text(value)
                  .FontSize(9);
           });
    }
}
