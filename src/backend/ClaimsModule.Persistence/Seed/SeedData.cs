using Microsoft.EntityFrameworkCore;
using ClaimsModule.Domain.Entities;
using ClaimsModule.Domain.Enumerations;

namespace ClaimsModule.Persistence.Seed;

public static class SeedData
{
    public static readonly Guid OrgId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public static readonly Guid Policy1Id = Guid.Parse("22222222-2222-2222-2222-222222222221");
    public static readonly Guid Policy2Id = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid Policy3Id = Guid.Parse("22222222-2222-2222-2222-222222222223");
    public static readonly Guid Policy4Id = Guid.Parse("22222222-2222-2222-2222-222222222224");
    public static readonly Guid PolicyExpiredId = Guid.Parse("22222222-2222-2222-2222-222222222229");

    public static void Seed(ModelBuilder modelBuilder)
    {
        // 1. Seed CauseOfLossCodes
        modelBuilder.Entity<CauseOfLossCode>().HasData(
            new CauseOfLossCode { CauseOfLossCodeId = Guid.Parse("33333333-3333-3333-3333-333333333301"), Code = "COL-FIRE", Name = "Fire", PerilCategory = "Property", IsActive = true, SortOrder = 1 },
            new CauseOfLossCode { CauseOfLossCodeId = Guid.Parse("33333333-3333-3333-3333-333333333302"), Code = "COL-FLOOD", Name = "Flood", PerilCategory = "Weather", IsActive = true, SortOrder = 2 },
            new CauseOfLossCode { CauseOfLossCodeId = Guid.Parse("33333333-3333-3333-3333-333333333303"), Code = "COL-THEFT", Name = "Theft", PerilCategory = "Crime", IsActive = true, SortOrder = 3 },
            new CauseOfLossCode { CauseOfLossCodeId = Guid.Parse("33333333-3333-3333-3333-333333333304"), Code = "COL-VEH-COL", Name = "Vehicle Collision", PerilCategory = "Auto", IsActive = true, SortOrder = 4 },
            new CauseOfLossCode { CauseOfLossCodeId = Guid.Parse("33333333-3333-3333-3333-333333333305"), Code = "COL-VEH-COMP", Name = "Vehicle Comprehensive", PerilCategory = "Auto", IsActive = true, SortOrder = 5 },
            new CauseOfLossCode { CauseOfLossCodeId = Guid.Parse("33333333-3333-3333-3333-333333333306"), Code = "COL-LIAB", Name = "Third Party Liability", PerilCategory = "Liability", IsActive = true, SortOrder = 6 },
            new CauseOfLossCode { CauseOfLossCodeId = Guid.Parse("33333333-3333-3333-3333-333333333307"), Code = "COL-EQUIP", Name = "Equipment Breakdown", PerilCategory = "Equipment", IsActive = true, SortOrder = 7 },
            new CauseOfLossCode { CauseOfLossCodeId = Guid.Parse("33333333-3333-3333-3333-333333333308"), Code = "COL-WIND", Name = "Wind / Storm", PerilCategory = "Weather", IsActive = true, SortOrder = 8 },
            new CauseOfLossCode { CauseOfLossCodeId = Guid.Parse("33333333-3333-3333-3333-333333333309"), Code = "COL-INJURY", Name = "Bodily Injury", PerilCategory = "Liability", IsActive = true, SortOrder = 9 },
            new CauseOfLossCode { CauseOfLossCodeId = Guid.Parse("33333333-3333-3333-3333-333333333310"), Code = "COL-OTHER", Name = "Other / Unknown", PerilCategory = "General", IsActive = true, SortOrder = 10 }
        );

        // 2. Seed Policies (Simulated)
        modelBuilder.Entity<Policy>().HasData(
            new Policy 
            { 
                PolicyId = Policy1Id, 
                PolicyNumber = "POL-2024-001001", 
                ClientName = "Meridian Transport LLC", 
                EffectiveDate = new DateOnly(2024, 1, 1), 
                ExpirationDate = new DateOnly(2026, 12, 31), 
                Status = "Active", 
                CoverageTypes = "Vehicle, Cargo",
                OrganizationEntityId = OrgId
            },
            new Policy 
            { 
                PolicyId = Policy2Id, 
                PolicyNumber = "POL-2024-001002", 
                ClientName = "Harborview Properties Inc", 
                EffectiveDate = new DateOnly(2024, 6, 1), 
                ExpirationDate = new DateOnly(2026, 5, 31), 
                Status = "Active", 
                CoverageTypes = "Property, Liability",
                OrganizationEntityId = OrgId
            },
            new Policy 
            { 
                PolicyId = Policy3Id, 
                PolicyNumber = "POL-2025-002001", 
                ClientName = "Coastal Builders Group", 
                EffectiveDate = new DateOnly(2025, 3, 1), 
                ExpirationDate = new DateOnly(2027, 2, 28), 
                Status = "Active", 
                CoverageTypes = "Property, Equipment",
                OrganizationEntityId = OrgId
            },
            new Policy 
            { 
                PolicyId = Policy4Id, 
                PolicyNumber = "POL-2025-002002", 
                ClientName = "Stanton Medical Group", 
                EffectiveDate = new DateOnly(2025, 1, 1), 
                ExpirationDate = new DateOnly(2026, 12, 31), 
                Status = "Active", 
                CoverageTypes = "Liability, Vehicle",
                OrganizationEntityId = OrgId
            },
            new Policy 
            { 
                PolicyId = PolicyExpiredId, 
                PolicyNumber = "POL-2023-000099", 
                ClientName = "Archived Corp", 
                EffectiveDate = new DateOnly(2020, 1, 1), 
                ExpirationDate = new DateOnly(2021, 12, 31), 
                Status = "Expired", 
                CoverageTypes = "Property",
                OrganizationEntityId = OrgId
            }
        );

        // 3. Seed ClaimStatusTransitions
        var tId = 1;
        modelBuilder.Entity<ClaimStatusTransition>().HasData(
            new ClaimStatusTransition { Id = Guid.Parse($"44444444-4444-4444-4444-4444444444{tId++:D2}"), FromStatus = ClaimStatus.Draft, ToStatus = ClaimStatus.Open, RequiredPermission = "handler" },
            new ClaimStatusTransition { Id = Guid.Parse($"44444444-4444-4444-4444-4444444444{tId++:D2}"), FromStatus = ClaimStatus.Open, ToStatus = ClaimStatus.UnderInvestigation, RequiredPermission = "handler" },
            new ClaimStatusTransition { Id = Guid.Parse($"44444444-4444-4444-4444-4444444444{tId++:D2}"), FromStatus = ClaimStatus.Open, ToStatus = ClaimStatus.PendingPayment, RequiredPermission = "handler" },
            new ClaimStatusTransition { Id = Guid.Parse($"44444444-4444-4444-4444-4444444444{tId++:D2}"), FromStatus = ClaimStatus.Open, ToStatus = ClaimStatus.Closed, RequiredPermission = "handler" },
            new ClaimStatusTransition { Id = Guid.Parse($"44444444-4444-4444-4444-4444444444{tId++:D2}"), FromStatus = ClaimStatus.Open, ToStatus = ClaimStatus.Withdrawn, RequiredPermission = "handler" },
            
            new ClaimStatusTransition { Id = Guid.Parse($"44444444-4444-4444-4444-4444444444{tId++:D2}"), FromStatus = ClaimStatus.UnderInvestigation, ToStatus = ClaimStatus.Open, RequiredPermission = "handler" },
            new ClaimStatusTransition { Id = Guid.Parse($"44444444-4444-4444-4444-4444444444{tId++:D2}"), FromStatus = ClaimStatus.UnderInvestigation, ToStatus = ClaimStatus.PendingPayment, RequiredPermission = "handler" },
            new ClaimStatusTransition { Id = Guid.Parse($"44444444-4444-4444-4444-4444444444{tId++:D2}"), FromStatus = ClaimStatus.UnderInvestigation, ToStatus = ClaimStatus.Closed, RequiredPermission = "handler" },
            new ClaimStatusTransition { Id = Guid.Parse($"44444444-4444-4444-4444-4444444444{tId++:D2}"), FromStatus = ClaimStatus.UnderInvestigation, ToStatus = ClaimStatus.Withdrawn, RequiredPermission = "handler" },
            
            new ClaimStatusTransition { Id = Guid.Parse($"44444444-4444-4444-4444-4444444444{tId++:D2}"), FromStatus = ClaimStatus.PendingPayment, ToStatus = ClaimStatus.Closed, RequiredPermission = "handler" },
            
            new ClaimStatusTransition { Id = Guid.Parse($"44444444-4444-4444-4444-4444444444{tId++:D2}"), FromStatus = ClaimStatus.Closed, ToStatus = ClaimStatus.Reopened, RequiredPermission = "supervisor" },
            new ClaimStatusTransition { Id = Guid.Parse($"44444444-4444-4444-4444-4444444444{tId++:D2}"), FromStatus = ClaimStatus.Reopened, ToStatus = ClaimStatus.Open, RequiredPermission = "handler" }
        );
    }
}
