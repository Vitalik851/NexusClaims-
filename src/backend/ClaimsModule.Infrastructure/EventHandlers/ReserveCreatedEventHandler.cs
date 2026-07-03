using MediatR;
using Hangfire;
using ClaimsModule.Domain.Events;
using ClaimsModule.Domain.Enumerations;
using ClaimsModule.Infrastructure.Jobs;

namespace ClaimsModule.Infrastructure.EventHandlers;

public class ReserveCreatedEventHandler : INotificationHandler<ReserveCreatedEvent>
{
    private readonly IBackgroundJobClient _backgroundJobClient;

    public ReserveCreatedEventHandler(IBackgroundJobClient backgroundJobClient)
    {
        _backgroundJobClient = backgroundJobClient;
    }

    public Task Handle(ReserveCreatedEvent notification, CancellationToken cancellationToken)
    {
        // Only auto-approved reserves should be automatically queued for GL posting
        if (notification.Status == ApprovalStatus.AutoApproved)
        {
            _backgroundJobClient.Enqueue<IGeneralLedgerPostJob>(job => 
                job.PostAsync(notification.ReserveHistoryId, CancellationToken.None));
        }

        return Task.CompletedTask;
    }
}
