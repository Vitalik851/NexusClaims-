using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Hangfire;
using ClaimsModule.Domain.Interfaces;
using ClaimsModule.Infrastructure.Storage;
using ClaimsModule.Infrastructure.Jobs;

namespace ClaimsModule.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. Register File Storage based on StorageProvider config key (AzureBlob | LocalFileSystem)
        var provider = configuration["StorageProvider"];
        if (provider == "AzureBlob")
        {
            services.AddSingleton<AzureBlobStorageService>();
            services.AddSingleton<IStorageService>(sp => sp.GetRequiredService<AzureBlobStorageService>());
        }
        else
        {
            services.AddSingleton<LocalStorageService>();
            services.AddSingleton<IStorageService>(sp => sp.GetRequiredService<LocalStorageService>());
        }

        // 2. Configure Hangfire SQL Server backend
        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSqlServerStorage(configuration.GetConnectionString("DefaultConnection")));

        // Add the Hangfire server process to host background jobs
        services.AddHangfireServer();

        // 3. Register background jobs
        services.AddScoped<IGeneralLedgerPostJob, GeneralLedgerPostJob>();
        services.AddScoped<ISlaMonitoringJob, SlaMonitoringJob>();

        return services;
    }
}
