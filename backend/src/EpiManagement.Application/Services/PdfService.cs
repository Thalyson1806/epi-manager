using EpiManagement.Domain.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Elements.Table;

namespace EpiManagement.Application.Services;

public class PdfService
{
    private readonly IUnitOfWork _uow;

    public PdfService(IUnitOfWork uow)
    {
        _uow = uow;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<byte[]> GenerateEmployeeEpiCardAsync(
        Guid employeeId, DateTime? startDate, DateTime? endDate, CancellationToken ct = default)
    {
        var emp = await _uow.Employees.GetByIdAsync(employeeId, ct)
            ?? throw new KeyNotFoundException("Funcionário não encontrado.");

        var deliveries = (await _uow.EpiDeliveries.GetByEmployeeAsync(employeeId, ct))
            .Where(d => (!startDate.HasValue || d.DeliveryDate >= startDate.Value)
                     && (!endDate.HasValue   || d.DeliveryDate <= endDate.Value))
            .OrderByDescending(d => d.DeliveryDate)
            .ToList();

        // Achata itens em linhas
        var rows = deliveries
            .SelectMany(d => d.Items.Select(i => (Delivery: d, Item: i)))
            .ToList();

        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.MarginHorizontal(1.4f, Unit.Centimetre);
                page.MarginVertical(1.2f, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Arial"));

                page.Content().Column(col =>
                {
                    // ── Título ────────────────────────────────────────────────
                    col.Item().BorderBottom(1).PaddingBottom(4).Column(c =>
                    {
                        c.Item().AlignCenter().Text("CONTROLE DE USO DE EQUIPAMENTOS")
                            .Bold().FontSize(14);
                        c.Item().AlignCenter().Text("PROTEÇÃO INDIVIDUAL (EPI15)")
                            .Bold().FontSize(12);
                    });

                    // ── NR-6 ──────────────────────────────────────────────────
                    col.Item().PaddingTop(6).Row(row =>
                    {
                        row.RelativeItem(3).Text(
                            "Conforme determinações da Norma Regulamentadora – NR-6, deve ser aberto uma ficha de controle de entrega dos " +
                            "Equipamentos de Proteção Individual – EPI's, fornecidos aos empregados, e estes, a assinarem a cada vez que retirar " +
                            "um novo EPI ou efetuar uma troca.")
                            .FontSize(8).Italic();
                        row.ConstantItem(8);
                        row.RelativeItem(1).AlignRight().AlignBottom()
                            .Text("NORMA REGULAMENTADORA").Bold().FontSize(8).Underline();
                    });

                    // ── Declaração de Recebimento ─────────────────────────────
                    col.Item().PaddingTop(8).AlignCenter().Text("DECLARAÇÃO DE RECEBIMENTO")
                        .Bold().FontSize(10).Underline();

                    col.Item().PaddingTop(4).Text(
                        "Declaro ter recebido gratuitamente, após orientação de uso, realizada pela CIPA, os Equipamentos de Proteção Individual " +
                        "abaixo descritos, os quais abrigo-me a usá-los em meu trabalho, sendo que os mesmos ficarão sob minha responsabilidade, " +
                        "autorizando o desconto caso eu os perca ou os destrua.")
                        .FontSize(8).Italic();

                    col.Item().PaddingTop(3).Text(
                        "Também estou ciente que a não utilização dos mesmos em minhas atividades profissionais, é ato faltoso e passível de punições " +
                        "legais e disciplinares, de acordo com a Consolidação das Leis do Trabalho (CLT) - Capítulo V – Seção I - Art. 158.")
                        .FontSize(8).Italic();

                    col.Item().PaddingTop(8).BorderTop(1).BorderBottom(1).PaddingVertical(4).Row(row =>
                    {
                        row.RelativeItem(4).Text(text =>
                        {
                            text.Span("NOME: ").Bold();
                            text.Span(emp.Name.ToUpper());
                            text.Span("    FUNÇÃO: ").Bold();
                            text.Span(emp.Position.ToUpper());
                        });
                        row.RelativeItem(1).AlignRight().Text(text =>
                        {
                            text.Span("MF  REGISTRO: ").Bold();
                            text.Span(emp.Registration);
                        });
                    });

                    // ── Tabela de EPIs ────────────────────────────────────────
                    col.Item().PaddingTop(6).Table(t =>
                    {
                        t.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(4); // DESCRIÇÃO
                            c.RelativeColumn(1); // QUANT.
                            c.RelativeColumn(1); // TROCA SIM
                            c.RelativeColumn(1); // TROCA NÃO
                            c.RelativeColumn(2); // DATA
                            c.RelativeColumn(2); // CA
                            c.RelativeColumn(3); // ASSINATURA
                        });

                        t.Header(h =>
                        {
                            h.Cell().Border(0.5f).PaddingHorizontal(3).PaddingVertical(3).Text("DESCRIÇÃO").Bold().FontSize(8);
                            h.Cell().Border(0.5f).PaddingHorizontal(3).PaddingVertical(3).Text("QUANT.").Bold().FontSize(8).AlignCenter();
                            h.Cell().ColumnSpan(2).Border(0.5f).PaddingVertical(2).Column(c2 =>
                            {
                                c2.Item().AlignCenter().Text("TROCA").Bold().FontSize(8);
                                c2.Item().Row(r2 =>
                                {
                                    r2.RelativeItem().BorderTop(0.5f).AlignCenter().Text("SIM").Bold().FontSize(8);
                                    r2.RelativeItem().BorderTop(0.5f).BorderLeft(0.5f).AlignCenter().Text("NÃO").Bold().FontSize(8);
                                });
                            });
                            h.Cell().Border(0.5f).PaddingHorizontal(3).PaddingVertical(3).Text("DATA").Bold().FontSize(8);
                            h.Cell().Border(0.5f).PaddingHorizontal(3).PaddingVertical(3).Text("CA").Bold().FontSize(8);
                            h.Cell().Border(0.5f).PaddingHorizontal(3).PaddingVertical(3).Text("ASSINATURA").Bold().FontSize(8);
                        });

                        int minRows = Math.Max(15, rows.Count);
                        for (int idx = 0; idx < minRows; idx++)
                        {
                            bool hasData = idx < rows.Count;
                            var delivery = hasData ? rows[idx].Delivery : null;
                            var item     = hasData ? rows[idx].Item     : null;

                            void DCell(string text = "", bool center = false)
                            {
                                var tb = t.Cell().Border(0.5f).MinHeight(18)
                                    .PaddingHorizontal(3).PaddingVertical(2)
                                    .Text(text).FontSize(8);
                                if (center) tb.AlignCenter();
                            }

                            DCell(item?.Epi?.Name ?? "");
                            DCell(item != null ? item.Quantity.ToString() : "", true);
                            DCell("", true); // TROCA SIM — preenche manualmente
                            DCell("", true); // TROCA NÃO — preenche manualmente
                            DCell(delivery != null ? delivery.DeliveryDate.ToString("dd/MM/yyyy") : "");
                            DCell(item?.Epi?.Code ?? "");

                            // ASSINATURA — selo biométrico gov.br style
                            if (delivery?.BiometricSignature != null)
                            {
                                t.Cell().Border(0.5f).MinHeight(18)
                                    .PaddingHorizontal(4).PaddingVertical(3).Column(sig =>
                                    {
                                        sig.Item().BorderBottom(0.5f).PaddingBottom(1)
                                            .Text(emp.Name.ToUpper()).Bold().FontSize(7);
                                        sig.Item().Text($"R.E.: {emp.Registration}").FontSize(6);
                                        sig.Item().Text("Assinado via biometria digital")
                                            .FontSize(6).Italic();
                                        sig.Item()
                                            .Text(delivery.DeliveryDate.ToString("dd/MM/yyyy HH:mm"))
                                            .FontSize(6);
                                    });
                            }
                            else
                            {
                                DCell();
                            }
                        }
                    });
                });

                page.Footer().Row(row =>
                {
                    row.RelativeItem().Text($"RH - Metalurgica Formigari   |   Gerado em {DateTime.Now:dd/MM/yyyy HH:mm}")
                        .FontSize(7);
                    row.RelativeItem().AlignRight().Text(t =>
                    {
                        t.Span("Página ").FontSize(7);
                        t.CurrentPageNumber().FontSize(7);
                        t.Span(" / ").FontSize(7);
                        t.TotalPages().FontSize(7);
                    });
                });
            });
        });

        return doc.GeneratePdf();
    }
}
