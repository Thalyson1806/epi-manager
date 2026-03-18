using EpiManagement.Domain.Interfaces;
using EpiManagement.Infrastructure.Biometric;
using EpiManagement.Infrastructure.Persistence;
using EpiManagement.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EpiManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(config.GetConnectionString("DefaultConnection")));

        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<ISectorRepository, SectorRepository>();
        services.AddScoped<IEpiRepository, EpiRepository>();
        services.AddScoped<IEpiDeliveryRepository, EpiDeliveryRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<ISystemConfigRepository, SystemConfigRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IBiometricService, DigitalPersonaService>();

        return services;
    }
}
