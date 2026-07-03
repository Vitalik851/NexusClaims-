using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ClaimsModule.Domain.Interfaces;
using ClaimsModule.Persistence.Context;
using ClaimsModule.Persistence.Repositories;
using ClaimsModule.Persistence.Services;

namespace ClaimsModule.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ClaimsDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ClaimsDbContext).Assembly.FullName)));

        services.AddScoped<IClaimRepository, ClaimRepository>();
        services.AddScoped<IReserveRepository, ReserveRepository>();
        services.AddScoped<IPolicyRepository, PolicyRepository>();
        services.AddScoped<ICauseOfLossCodeRepository, CauseOfLossCodeRepository>();
        services.AddScoped<IClaimStatusTransitionRepository, ClaimStatusTransitionRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<IClaimNumberGenerator, ClaimNumberGenerator>();

        return services;
    }
}
