using Microsoft.EntityFrameworkCore;
using ClaimsModule.Domain.Entities;
using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Events;

namespace ClaimsModule.Persistence.Context;

public class ClaimsDbContext : DbContext
{
    public ClaimsDbContext(DbContextOptions<ClaimsDbContext> options) : base(options)
    {
    }

    public DbSet<Claim> Claims => Set<Claim>();
    public DbSet<LossEvent> LossEvents => Set<LossEvent>();
    public DbSet<ClaimParty> ClaimParties => Set<ClaimParty>();
    public DbSet<ClaimRiskObject> ClaimRiskObjects => Set<ClaimRiskObject>();
    public DbSet<ClaimReserveComponent> ClaimReserveComponents => Set<ClaimReserveComponent>();
    public DbSet<ReserveHistory> ReserveHistories => Set<ReserveHistory>();
    public DbSet<ClaimDocument> ClaimDocuments => Set<ClaimDocument>();
    public DbSet<ClaimAuditLog> ClaimAuditLogs => Set<ClaimAuditLog>();
    public DbSet<CauseOfLossCode> CauseOfLossCodes => Set<CauseOfLossCode>();
    public DbSet<Policy> Policies => Set<Policy>();
    public DbSet<ClaimStatusTransition> ClaimStatusTransitions => Set<ClaimStatusTransition>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Ignore<DomainEvent>();
        
        modelBuilder.HasSequence<int>("ClaimNumberSequence")
            .StartsAt(1)
            .IncrementsBy(1);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ClaimsDbContext).Assembly);

        // Global query filter for Soft Delete
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");
                var propertyMethodInfo = typeof(EF).GetMethod("Property")?.MakeGenericMethod(typeof(bool));
                if (propertyMethodInfo != null)
                {
                    var isDeletedProperty = System.Linq.Expressions.Expression.Call(
                        propertyMethodInfo, 
                        parameter, 
                        System.Linq.Expressions.Expression.Constant("IsDeleted")
                    );
                    var compareExpression = System.Linq.Expressions.Expression.Equal(
                        isDeletedProperty, 
                        System.Linq.Expressions.Expression.Constant(false)
                    );
                    var lambda = System.Linq.Expressions.Expression.Lambda(compareExpression, parameter);
                    modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
                }
            }
        }

        Seed.SeedData.Seed(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var currentUserId = Guid.Parse("00000000-0000-0000-0000-000000000001"); // Simulating system/current user

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTimeOffset.UtcNow;
                    entry.Entity.UserCreated = currentUserId;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTimeOffset.UtcNow;
                    entry.Entity.UserModified = currentUserId;
                    break;
            }
        }

        foreach (var entry in ChangeTracker.Entries<ISoftDeletable>())
        {
            if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                entry.Entity.SoftDelete(DateTimeOffset.UtcNow);
            }
        }

        // Collect domain events before saving
        var domainEntities = ChangeTracker.Entries<BaseEntity>()
            .Where(x => x.Entity.DomainEvents != null && x.Entity.DomainEvents.Any())
            .ToList();

        var domainEvents = domainEntities
            .SelectMany(x => x.Entity.DomainEvents)
            .ToList();

        domainEntities.ForEach(x => x.Entity.ClearDomainEvents());

        var result = await base.SaveChangesAsync(cancellationToken);

        // Note: In real Clean Architecture, events are published here via MediatR.
        // We will inject IMediator or handle publishing in a SaveChanges interceptor or inside the command pipeline.
        // For simplicity, they can also be dispatched by the MediatR Pipeline/UnitOfWork.

        return result;
    }
}
