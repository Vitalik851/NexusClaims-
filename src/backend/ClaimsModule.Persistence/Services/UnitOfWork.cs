using ClaimsModule.Domain.Interfaces;
using ClaimsModule.Persistence.Context;
using ClaimsModule.Persistence.Repositories;

namespace ClaimsModule.Persistence.Services;

public class UnitOfWork : IUnitOfWork
{
    private readonly ClaimsDbContext _context;
    
    private IClaimRepository? _claims;
    private IReserveRepository? _reserves;
    private IPolicyRepository? _policies;
    private ICauseOfLossCodeRepository? _causeOfLossCodes;
    private IClaimStatusTransitionRepository? _statusTransitions;
    private IAuditLogRepository? _auditLogs;

    public UnitOfWork(ClaimsDbContext context)
    {
        _context = context;
    }

    public IClaimRepository Claims => _claims ??= new ClaimRepository(_context);
    public IReserveRepository Reserves => _reserves ??= new ReserveRepository(_context);
    public IPolicyRepository Policies => _policies ??= new PolicyRepository(_context);
    public ICauseOfLossCodeRepository CauseOfLossCodes => _causeOfLossCodes ??= new CauseOfLossCodeRepository(_context);
    public IClaimStatusTransitionRepository StatusTransitions => _statusTransitions ??= new ClaimStatusTransitionRepository(_context);
    public IAuditLogRepository AuditLogs => _auditLogs ??= new AuditLogRepository(_context);

    public async Task<int> SaveChangesAsync(CancellationToken ct)
    {
        return await _context.SaveChangesAsync(ct);
    }
}
