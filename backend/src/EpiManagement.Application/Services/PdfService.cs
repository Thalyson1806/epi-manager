using EpiManagement.Application.DTOs;
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

        var deliveries = await _uow.EpiDeliveries.GetByEmployeeAsync(employeeId, ct);

        if (startDate.HasValue)
            deliveries = deliveries.Where(d => d.DeliveryDate >= startDate.Value);
        if (endDate.HasValue)
            deliveries = deliveries.Where(d => d.DeliveryDate <= endDate.Value);

        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Column(col =>
                {
                    col.Item().Text("FICHA DE CONTROLE DE EPI")
                        .Bold().FontSize(16).AlignCenter();
                    col.Item().Text($"Gerado em: {DateTime.Now:dd/MM/yyyy HH:mm}")
                        .FontSize(8).AlignRight();
                });

                page.Content().Column(col =>
                {
                    col.Item().PaddingBottom(10).Table(t =>
                    {
                        t.ColumnsDefinition(c => { c.RelativeColumn(); c.RelativeColumn(); });
                        t.Cell().Text("Nome:").Bold();
                        t.Cell().Text(emp.Name);
                        t.Cell().Text("Matrícula:").Bold();
                        t.Cell().Text(emp.Registration);
                        t.Cell().Text("CPF:").Bold();
                        t.Cell().Text(emp.Cpf);
                        t.Cell().Text("Setor:").Bold();
                        t.Cell().Text(emp.Sector?.Name ?? "-");
                        t.Cell().Text("Cargo:").Bold();
                        t.Cell().Text(emp.Position);
                        t.Cell().Text("Admissão:").Bold();
                        t.Cell().Text(emp.AdmissionDate.ToString("dd/MM/yyyy"));
                    });

                    col.Item().PaddingBottom(5).Text("Histórico de Entregas").Bold().FontSize(12);

                    col.Item().Table(t =>
                    {
                        t.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(2);
                            c.RelativeColumn(3);
                            c.RelativeColumn(1);
                            c.RelativeColumn(2);
                            c.RelativeColumn(2);
                        });

                        t.Header(h =>
                        {
                            h.Cell().Background(Colors.Grey.Lighten2).Text("Data").Bold();
                            h.Cell().Background(Colors.Grey.Lighten2).Text("EPI").Bold();
                            h.Cell().Background(Colors.Grey.Lighten2).Text("Qtd").Bold();
                            h.Cell().Background(Colors.Grey.Lighten2).Text("Próx. Troca").Bold();
                            h.Cell().Background(Colors.Grey.Lighten2).Text("Assinatura").Bold();
                        });

                        foreach (var delivery in deliveries.OrderByDescending(d => d.DeliveryDate))
                        {
                            foreach (var item in delivery.Items)
                            {
                                t.Cell().Text(delivery.DeliveryDate.ToString("dd/MM/yyyy"));
                                t.Cell().Text(item.Epi?.Name ?? "-");
                                t.Cell().Text(item.Quantity.ToString());
                                t.Cell().Text(item.NextReplacementDate.ToString("dd/MM/yyyy"));
                                t.Cell().Text(delivery.BiometricSignature != null ? "Biometria" : "-");
                            }
                        }
                    });
                });

                page.Footer().Text(t =>
                {
                    t.Span("Página ");
                    t.CurrentPageNumber();
                    t.Span(" de ");
                    t.TotalPages();
                });
            });
        });

        return doc.GeneratePdf();
    }
}
