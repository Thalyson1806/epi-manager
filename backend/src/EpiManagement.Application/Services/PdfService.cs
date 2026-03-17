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

        var firstDelivery = allDeliveries.FirstOrDefault(d => d.IsFirstDelivery);
        var subsequentDeliveries = allDeliveries.Where(d => !d.IsFirstDelivery).ToList();

        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.MarginTop(1.2f, Unit.Centimetre);
                page.MarginBottom(1f, Unit.Centimetre);
                page.MarginHorizontal(1.5f, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(9));

                page.Content().Column(col =>
                {
                    // ── Cabeçalho ────────────────────────────────────────────
                    col.Item().Border(1).BorderColor(Colors.Black).Table(t =>
                    {
                        t.ColumnsDefinition(c => { c.RelativeColumn(3); c.RelativeColumn(1); });

                        t.Cell().RowSpan(2).Padding(6).Column(inner =>
                        {
                            inner.Item().Text("CONTROLE DE USO DE EQUIPAMENTOS")
                                .Bold().FontSize(12).AlignCenter();
                            inner.Item().Text("PROTEÇÃO INDIVIDUAL (EPI15)")
                                .Bold().FontSize(11).AlignCenter();
                        });

                        t.Cell().BorderLeft(1).BorderColor(Colors.Black).Padding(4).AlignRight()
                            .Text("NORMA REGULAMENTADORA").Bold().Underline().FontSize(9);

                        t.Cell().BorderLeft(1).BorderTop(1).BorderColor(Colors.Black).Padding(4).AlignRight()
                            .Text("NR-6 — Equipamento de Proteção Individual").FontSize(8).Italic();
                    });

                    col.Item().PaddingTop(4);

                    // ── Texto NR-6 ───────────────────────────────────────────
                    col.Item().Border(1).BorderColor(Colors.Black).Padding(6).Text(t =>
                    {
                        t.Span("Conforme determinações da Norma Regulamentadora – NR-6, deve ser aberto uma ficha de controle de entrega dos ");
                        t.Span("Equipamentos de Proteção Individual – EPI's").Bold();
                        t.Span(", fornecidos aos empregados, e estes, a assinarem a cada vez que retirar um novo EPI ou efetuar uma troca.");
                    });

                    col.Item().PaddingTop(4);

                    // ── Declaração de Recebimento ────────────────────────────
                    col.Item().Border(1).BorderColor(Colors.Black).Padding(6).Column(inner =>
                    {
                        inner.Item().PaddingBottom(4)
                            .Text("DECLARAÇÃO DE RECEBIMENTO")
                            .Bold().FontSize(10).Underline().AlignCenter();

                        inner.Item().Text(
                            "Declaro ter recebido gratuitamente, após orientação de uso, realizada pela CIPA, os Equipamentos de Proteção Individual " +
                            "abaixo descritos, os quais abrigo-me a usá-los em meu trabalho, sendo que os mesmos ficarão sob minha responsabilidade, " +
                            "autorizando o desconto caso eu os perca ou os destrua.");

                        inner.Item().PaddingTop(4).Text(
                            "Também estou ciente que a não utilização dos mesmos em minhas atividades profissionais, é ato faltoso e passível de " +
                            "punições legais e disciplinares, de acordo com a Consolidação das Leis do Trabalho (CLT) - Capítulo V – Seção I – Art. 158.");

                        inner.Item().PaddingTop(10).Row(row =>
                        {
                            row.RelativeItem().Column(dateCol =>
                            {
                                if (firstDelivery != null)
                                    dateCol.Item().Text(firstDelivery.DeliveryDate.ToLocalTime().ToString("dd/MM/yyyy"))
                                        .FontSize(9).AlignCenter();
                                else
                                    dateCol.Item().Text("_____ / _____ / _______").FontSize(9).AlignCenter();
                                dateCol.Item().BorderTop(1).BorderColor(Colors.Black)
                                    .PaddingTop(2).Text("Data").FontSize(8).AlignCenter();
                            });

                            row.ConstantItem(20);

                            row.RelativeItem(3).Column(sigCol =>
                            {
                                if (firstDelivery is { HasBiometricSignature: true })
                                {
                                    sigCol.Item().Border(1).BorderColor(Colors.Grey.Darken2)
                                        .Background(Colors.Grey.Lighten4).Padding(6).Column(seal =>
                                        {
                                            seal.Item().Text("✓  ASSINADO ELETRONICAMENTE VIA BIOMETRIA DIGITAL")
                                                .Bold().FontSize(7.5f).AlignCenter();
                                            seal.Item().PaddingTop(2)
                                                .Text(emp.Name.ToUpper()).Bold().FontSize(8).AlignCenter();
                                            seal.Item().Text($"R.E.: {emp.Registration}")
                                                .FontSize(7.5f).AlignCenter();
                                            seal.Item().Text(firstDelivery.DeliveryDate.ToLocalTime().ToString("dd/MM/yyyy HH:mm"))
                                                .FontSize(7).AlignCenter().FontColor(Colors.Grey.Darken1);
                                        });
                                }
                                else
                                {
                                    sigCol.Item().Height(40).BorderBottom(1).BorderColor(Colors.Black);
                                    sigCol.Item().PaddingTop(2).Text("Assinatura do Empregado").FontSize(8).AlignCenter();
                                }
                            });
                        });
                    });

                    col.Item().PaddingTop(4);

                    // ── Dados do funcionário ──────────────────────────────────
                    col.Item().Border(1).BorderColor(Colors.Black).Padding(5).Row(row =>
                    {
                        row.RelativeItem(3).Text(t =>
                        {
                            t.Span("NOME: ").Bold();
                            t.Span(emp.Name.ToUpper());
                        });
                        row.RelativeItem(2).Text(t =>
                        {
                            t.Span("FUNÇÃO: ").Bold();
                            t.Span(emp.Position.ToUpper());
                        });
                        row.RelativeItem().Text(t =>
                        {
                            t.Span("MF REGISTRO: ").Bold();
                            t.Span(emp.Registration);
                        });
                    });

                    col.Item().PaddingTop(4);

                    // ── Tabela de EPIs ────────────────────────────────────────
                    col.Item().Border(1).BorderColor(Colors.Black).Table(t =>
                    {
                        t.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(4); // Descrição
                            c.RelativeColumn(1); // Quant.
                            c.RelativeColumn(1); // Troca SIM
                            c.RelativeColumn(1); // Troca NÃO
                            c.RelativeColumn(2); // Data
                            c.RelativeColumn(1); // CA
                            c.RelativeColumn(2); // Assinatura
                        });

                        static IContainer HeaderCell(IContainer c) =>
                            c.Background(Colors.Grey.Lighten3).Border(1).BorderColor(Colors.Black)
                             .Padding(3).AlignCenter().AlignMiddle();

                        static IContainer DataCell(IContainer c) =>
                            c.Border(1).BorderColor(Colors.Black).Padding(3).AlignMiddle();

                        t.Header(h =>
                        {
                            h.Cell().RowSpan(2).Element(HeaderCell).Text("DESCRIÇÃO").Bold();
                            h.Cell().RowSpan(2).Element(HeaderCell).Text("QUANT.").Bold();
                            h.Cell().ColumnSpan(2).Element(HeaderCell).Text("TROCA").Bold();
                            h.Cell().RowSpan(2).Element(HeaderCell).Text("DATA").Bold();
                            h.Cell().RowSpan(2).Element(HeaderCell).Text("CA").Bold();
                            h.Cell().RowSpan(2).Element(HeaderCell).Text("ASSINATURA").Bold();
                            h.Cell().Element(HeaderCell).Text("SIM").Bold();
                            h.Cell().Element(HeaderCell).Text("NÃO").Bold();
                        });

                        // Primeira entrega — assinatura já está no termo
                        if (firstDelivery != null)
                        {
                            foreach (var item in firstDelivery.Items)
                            {
                                t.Cell().Element(DataCell).Text(item.EpiName);
                                t.Cell().Element(DataCell).AlignCenter().Text(item.Quantity.ToString());
                                t.Cell().Element(DataCell).AlignCenter().Text(""); // Troca SIM
                                t.Cell().Element(DataCell).AlignCenter().Text("X"); // Troca NÃO (recebimento novo)
                                t.Cell().Element(DataCell).AlignCenter()
                                    .Text(firstDelivery.DeliveryDate.ToLocalTime().ToString("dd/MM/yyyy"));
                                t.Cell().Element(DataCell).AlignCenter().Text("—");
                                t.Cell().Element(DataCell).AlignCenter()
                                    .Text(firstDelivery.HasBiometricSignature ? $"✓ Bio\n{emp.Registration}" : "").FontSize(7);
                            }
                        }

                        // Trocas subsequentes
                        foreach (var delivery in subsequentDeliveries)
                        {
                            foreach (var item in delivery.Items)
                            {
                                var desc = item.EpiName;
                                if (item.IsEarlyReplacement)
                                    desc += "\n★ Troca antecipada";

                                t.Cell().Element(DataCell).Text(desc).FontSize(8);
                                t.Cell().Element(DataCell).AlignCenter().Text(item.Quantity.ToString());
                                t.Cell().Element(DataCell).AlignCenter().Text("X"); // Troca SIM
                                t.Cell().Element(DataCell).AlignCenter().Text("");
                                t.Cell().Element(DataCell).AlignCenter()
                                    .Text(delivery.DeliveryDate.ToLocalTime().ToString("dd/MM/yyyy"));
                                t.Cell().Element(DataCell).AlignCenter().Text("—");
                                t.Cell().Element(DataCell).AlignCenter()
                                    .Text(delivery.HasBiometricSignature ? $"✓ Bio\n{emp.Registration}" : "").FontSize(7);
                            }
                        }

                        // Linhas em branco
                        int used = (firstDelivery?.Items.Count() ?? 0)
                            + subsequentDeliveries.Sum(d => d.Items.Count());
                        int blanks = Math.Max(5, 12 - used);
                        for (int i = 0; i < blanks; i++)
                            for (int c = 0; c < 7; c++)
                                t.Cell().Element(DataCell).Text(" ").FontSize(10);
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
                    t.Span("Página ").FontSize(8);
                    t.CurrentPageNumber().FontSize(8);
                    t.Span(" de ").FontSize(8);
                    t.TotalPages().FontSize(8);
                });
            });
        });

        return doc.GeneratePdf();
    }
}
