using MediatR;
using Hangfire;
using ClaimsModule.Domain.Events;
using ClaimsModule.Infrastructure.Jobs;

namespace ClaimsModule.Infrastructure.EventHandlers;

public class ReserveApprovedEventHandler : INotificationHandler<ReserveApprovedEvent>
{
    private readonly IBackgroundJobClient _backgroundJobClient;

    public ReserveApprovedEventHandler(IBackgroundJobClient backgroundJobClient)
    {
        _backgroundJobClient = backgroundJobClient;
    }

    public Task Handle(ReserveApprovedEvent notification, CancellationToken cancellationToken)
    {
        // Enqueue background posting task to Hangfire
        _backgroundJobClient.Enqueue<IGeneralLedgerPostJob>(job => 
            job.PostAsync(notification.ReserveHistoryId, CancellationToken.None));

        return Task.CompletedTask;
    }
}
