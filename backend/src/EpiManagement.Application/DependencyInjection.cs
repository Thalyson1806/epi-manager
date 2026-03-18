using EpiManagement.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EpiManagement.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<EmployeeService>();
        services.AddScoped<SectorService>();
        services.AddScoped<EpiService>();
        services.AddScoped<EpiDeliveryService>();
        services.AddScoped<AuthService>();
        services.AddScoped<PdfService>();
        services.AddScoped<EmailService>();
        services.AddScoped<AlertService>();
        services.AddHostedService<EpiAlertJob>();
        return services;
    }
}
