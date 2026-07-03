namespace ClaimsModule.Infrastructure.Jobs;

/// <summary>
/// Background job that monitors claims for SLA breaches.
/// </summary>
public interface ISlaMonitoringJob
{
    /// <summary>
    /// Executes the SLA scanning and logging process.
    /// </summary>
    Task ExecuteAsync(CancellationToken ct = default);
}
