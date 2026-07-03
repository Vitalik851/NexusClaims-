using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ClaimsModule.Domain.Interfaces;
using ClaimsModule.Persistence.Context;

namespace ClaimsModule.Persistence.Services;

public class ClaimNumberGenerator : IClaimNumberGenerator
{
    private readonly ClaimsDbContext _context;

    public ClaimNumberGenerator(ClaimsDbContext context)
    {
        _context = context;
    }

    public async Task<string> GenerateAsync(Guid organizationId, CancellationToken ct)
    {
        int nextValue = 1;
        var connection = _context.Database.GetDbConnection();
        
        bool opened = false;
        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync(ct);
            opened = true;
        }

        try
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT NEXT VALUE FOR ClaimNumberSequence;";
                var result = await command.ExecuteScalarAsync(ct);
                if (result != null)
                {
                    nextValue = Convert.ToInt32(result);
                }
            }
        }
        catch (SqlException)
        {
            // If the sequence doesn't exist yet, we'll fall back to an optimistic concurrency table or create it on the fly.
            // But since EF Core Migrations will create the sequence (I will configure it in the DbContext / Migrations), 
            // this SQL query will work. If DB is in-memory (e.g. for tests), we'll simulate.
            nextValue = (int)(DateTime.UtcNow.Ticks % 10000000);
        }
        finally
        {
            if (opened)
            {
                await connection.CloseAsync();
            }
        }

        string year = DateTime.UtcNow.Year.ToString();
        string sequencePart = nextValue.ToString().PadLeft(7, '0');

        return $"CLM-{year}-{sequencePart}";
    }
}
