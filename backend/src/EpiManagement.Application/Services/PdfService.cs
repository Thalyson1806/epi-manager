using EpiManagement.Domain.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace EpiManagement.Application.Services;

public class PdfService
{
    private readonly IUnitOfWork _uow;
    private readonly EpiDeliveryService _deliveryService;

    public PdfService(IUnitOfWork uow, EpiDeliveryService deliveryService)
    {
        _uow = uow;
        _deliveryService = deliveryService;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<byte[]> GenerateEmployeeEpiCardAsync(Guid employeeId, DateTime? startDate, DateTime? endDate, CancellationToken ct = default)
    {
        var emp = await _uow.Employees.GetByIdAsync(employeeId, ct)
            ?? throw new KeyNotFoundException("Funcionário não encontrado.");

        var allDeliveries = (await _deliveryService.GetByEmployeeAsync(employeeId, ct))
            .OrderBy(d => d.DeliveryDate).ToList();

        if (startDate.HasValue)
            allDeliveries = allDeliveries.Where(d => d.DeliveryDate >= startDate.Value).ToList();
        if (endDate.HasValue)
            allDeliveries = allDeliveries.Where(d => d.DeliveryDate <= endDate.Value).ToList();

        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.MarginTop(1.5f, Unit.Centimetre);
                page.MarginBottom(1.5f, Unit.Centimetre);
                page.MarginHorizontal(1.5f, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(9));

                page.Content().Column(col =>
                {
                    // ── Título ────────────────────────────────────────────────
                    col.Item().AlignCenter().Text("CONTROLE DE USO DE EQUIPAMENTOS")
                        .Bold().FontSize(14);
                    col.Item().AlignCenter().Text("PROTEÇÃO INDIVIDUAL (EPI15)")
                        .Bold().FontSize(12);
                    col.Item().PaddingTop(4).LineHorizontal(1).LineColor(Colors.Black);
                    col.Item().PaddingTop(4);

                    // ── Bloco NR-6 + Declaração ───────────────────────────────
                    col.Item().Border(1).BorderColor(Colors.Black).Column(block =>
                    {
                        // NR-6 text row
                        block.Item().Padding(6).Row(row =>
                        {
                            row.RelativeItem(3).Text(t =>
                            {
                                t.Span("Conforme determinações da Norma Regulamentadora – NR-6, deve ser aberto uma ficha de controle de entrega dos ").Italic().FontSize(8);
                                t.Span("Equipamentos de Proteção Individual – EPI's").Bold().Italic().FontSize(8);
                                t.Span(", fornecidos aos empregados, e estes, a assinarem a cada vez que retirar um novo EPI ou efetuar uma troca.").Italic().FontSize(8);
                            });
                            row.ConstantItem(8);
                            row.RelativeItem(1).AlignRight().AlignBottom()
                                .Text("NORMA REGULAMENTADORA").Bold().Underline().FontSize(8);
                        });

                        // Declaração title
                        block.Item()
                            .PaddingTop(2).PaddingBottom(4)
                            .AlignCenter()
                            .Text("DECLARAÇÃO DE RECEBIMENTO")
                            .Bold().FontSize(10).Underline();

                        // Paragraph 1
                        block.Item().PaddingHorizontal(6).PaddingBottom(4).Text(t =>
                        {
                            t.Span("Declaro ter recebido gratuitamente, após orientação de uso, realizada pela CIPA, os ").Italic().FontSize(8);
                            t.Span("Equipamentos de Proteção Individual").Bold().Italic().FontSize(8);
                            t.Span(" abaixo descritos, os quais abrigo-me a usá-los em meu trabalho, sendo que os mesmos ficarão sob minha responsabilidade, autorizando o desconto caso eu os perca ou os destrua.").Italic().FontSize(8);
                        });

                        // Paragraph 2
                        block.Item().PaddingHorizontal(6).PaddingBottom(10).Text(
                            "Também estou ciente que a não utilização dos mesmos em minhas atividades profissionais, é ato faltoso e passível de punições legais e disciplinares, de acordo com a Consolidação das Leis do Trabalho (CLT) - Capítulo V – Seção I – Art. 158.")
                            .Italic().FontSize(8);

                        // Date + Signature
                        var firstDelivery = allDeliveries.FirstOrDefault(d => d.IsFirstDelivery);
                        block.Item().PaddingHorizontal(6).PaddingBottom(8).Row(row =>
                        {
                            // Date
                            if (firstDelivery != null)
                                row.ConstantItem(90).Text(firstDelivery.DeliveryDate.ToLocalTime().ToString("dd/MM/yyyy")).FontSize(9);
                            else
                                row.ConstantItem(90).Text("____/______/________").FontSize(9);

                            row.RelativeItem();

                            // Signature
                            if (firstDelivery is { HasBiometricSignature: true })
                            {
                                row.ConstantItem(220).Column(sig =>
                                {
                                    sig.Item().Text(emp.Name.ToUpper()).Bold().FontSize(8);
                                    sig.Item().Text($"R.E.: {emp.Registration}").FontSize(7.5f);
                                    sig.Item().Text("Assinado via biometria digital")
                                        .Italic().FontSize(7).FontColor("#C0603A");
                                    sig.Item().Text(firstDelivery.DeliveryDate.ToLocalTime().ToString("dd/MM/yyyy HH:mm"))
                                        .FontSize(7).FontColor("#C0603A");
                                    sig.Item().PaddingTop(2).BorderBottom(1).BorderColor(Colors.Black);
                                    sig.Item().PaddingTop(2).AlignCenter().Text("Assinatura do Empregado").FontSize(8);
                                });
                            }
                            else
                            {
                                row.ConstantItem(220).Column(sig =>
                                {
                                    sig.Item().Height(22);
                                    sig.Item().BorderBottom(1).BorderColor(Colors.Black);
                                    sig.Item().PaddingTop(2).AlignCenter()
                                        .Text("Assinatura do Empregado").FontSize(8);
                                });
                            }

                            row.RelativeItem();
                        });
                    });

                    col.Item().PaddingTop(8);

                    // ── Dados do funcionário ──────────────────────────────────
                    col.Item().Row(row =>
                    {
                        row.AutoItem().Text(t =>
                        {
                            t.Span("NOME: ").Bold().FontSize(9);
                            t.Span(emp.Name.ToUpper()).Bold().FontSize(9);
                        });
                        row.ConstantItem(14);
                        row.AutoItem().Text(t =>
                        {
                            t.Span("FUNÇÃO: ").Bold().FontSize(9);
                            t.Span(emp.Position.ToUpper()).Bold().FontSize(9);
                        });
                        if (!string.IsNullOrEmpty(emp.WorkShift))
                        {
                            row.ConstantItem(10);
                            row.AutoItem().Text(emp.WorkShift.ToUpper()).Bold().FontSize(9);
                        }
                        row.RelativeItem();
                        row.AutoItem().Text(t =>
                        {
                            t.Span("MF  REGISTRO: ").Bold().FontSize(9);
                            t.Span(emp.Registration).Bold().FontSize(9);
                        });
                    });

                    col.Item().PaddingTop(3).LineHorizontal(1).LineColor(Colors.Black);
                    col.Item().PaddingTop(4);

                    // ── Tabela de EPIs ────────────────────────────────────────
                    col.Item().Border(1).BorderColor(Colors.Black).Table(t =>
                    {
                        t.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(5);    // Descrição
                            c.RelativeColumn(1.5f); // Quant.
                            c.RelativeColumn(1.2f); // SIM
                            c.RelativeColumn(1.2f); // NÃO
                            c.RelativeColumn(2.2f); // Data
                            c.RelativeColumn(2f);   // CA
                            c.RelativeColumn(3.5f); // Assinatura
                        });

                        IContainer HeaderCell(IContainer c) =>
                            c.Border(1).BorderColor(Colors.Black)
                             .Padding(3).AlignCenter().AlignMiddle();

                        IContainer DataCell(IContainer c) =>
                            c.Border(1).BorderColor(Colors.Black).Padding(3).AlignMiddle();

                        t.Header(h =>
                        {
                            h.Cell().RowSpan(2).Element(HeaderCell).Text("DESCRIÇÃO").Bold().FontSize(8);
                            h.Cell().RowSpan(2).Element(HeaderCell).Text("QUANT.").Bold().FontSize(8);
                            h.Cell().ColumnSpan(2).Element(HeaderCell).Text("TROCA").Bold().FontSize(8);
                            h.Cell().RowSpan(2).Element(HeaderCell).Text("DATA").Bold().FontSize(8);
                            h.Cell().RowSpan(2).Element(HeaderCell).Text("CA").Bold().FontSize(8);
                            h.Cell().RowSpan(2).Element(HeaderCell).Text("ASSINATURA").Bold().FontSize(8);
                            h.Cell().Element(HeaderCell).Text("SIM").Bold().FontSize(8);
                            h.Cell().Element(HeaderCell).Text("NÃO").Bold().FontSize(8);
                        });

                        int rowCount = 0;
                        foreach (var delivery in allDeliveries)
                        {
                            foreach (var item in delivery.Items)
                            {
                                rowCount++;
                                bool isReplacement = !delivery.IsFirstDelivery || item.IsEarlyReplacement;

                                t.Cell().Element(DataCell).Text(item.EpiName).FontSize(8);
                                t.Cell().Element(DataCell).AlignCenter().Text(item.Quantity.ToString()).FontSize(8);
                                t.Cell().Element(DataCell).AlignCenter().Text(isReplacement ? "X" : "").FontSize(9);
                                t.Cell().Element(DataCell).AlignCenter().Text("").FontSize(8);
                                t.Cell().Element(DataCell).AlignCenter()
                                    .Text(delivery.DeliveryDate.ToLocalTime().ToString("dd/MM/yyyy")).FontSize(8);
                                t.Cell().Element(DataCell).AlignCenter().Text(item.EpiCode).FontSize(8);

                                if (delivery.HasBiometricSignature)
                                {
                                    t.Cell().Element(DataCell).Column(sig =>
                                    {
                                        sig.Item().Text(emp.Name.ToUpper()).Bold().FontSize(7);
                                        sig.Item().Text($"R.E.: {emp.Registration}").FontSize(7);
                                        sig.Item().Text("Assinado via biometria digital")
                                            .Italic().FontSize(6.5f).FontColor("#C0603A");
                                        sig.Item().Text(delivery.DeliveryDate.ToLocalTime().ToString("dd/MM/yyyy HH:mm"))
                                            .FontSize(6.5f).FontColor("#C0603A");
                                    });
                                }
                                else
                                {
                                    t.Cell().Element(DataCell).MinHeight(18).Text("").FontSize(8);
                                }
                            }
                        }

                        // Blank rows
                        int blanks = Math.Max(8, 15 - rowCount);
                        for (int i = 0; i < blanks; i++)
                            for (int c = 0; c < 7; c++)
                                t.Cell().Element(DataCell).MinHeight(18).Text("").FontSize(8);
                    });

                    // ── Observações: trocas antecipadas ──────────────────────
                    var earlyItems = allDeliveries
                        .SelectMany(d => d.Items
                            .Where(i => i.IsEarlyReplacement && !string.IsNullOrEmpty(i.EarlyReplacementReason))
                            .Select(i => (item: i, date: d.DeliveryDate)))
                        .ToList();

                    if (earlyItems.Any())
                    {
                        col.Item().PaddingTop(6).Border(1).BorderColor(Colors.Black).Padding(5).Column(obs =>
                        {
                            obs.Item().Text("OBSERVAÇÕES — TROCAS ANTECIPADAS").Bold().FontSize(9);
                            foreach (var (item, date) in earlyItems)
                                obs.Item().PaddingTop(2).Text(t =>
                                {
                                    t.Span($"• {date.ToLocalTime():dd/MM/yyyy} – {item.EpiName}: ").Bold().FontSize(8);
                                    t.Span(item.EarlyReplacementReason).FontSize(8);
                                });
                        });
                    }
                });

                page.Footer().AlignRight().Text(t =>
                {
                    t.Span("Página ").FontSize(7);
                    t.CurrentPageNumber().FontSize(7);
                    t.Span(" de ").FontSize(7);
                    t.TotalPages().FontSize(7);
                });
            });
        });

        return doc.GeneratePdf();
    }
}
