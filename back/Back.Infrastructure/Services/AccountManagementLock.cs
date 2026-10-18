using Back.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Threading.Tasks;

namespace Back.Infrastructure.Services;

public static class AccountManagementLock
{
    // Transaction-scoped PostgreSQL lock, shared by all API instances and account mutations.
    public static async Task<IDbContextTransaction> AcquireAsync(ApplicationDbContext db)
    {
        var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            await db.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(5042026)");
            return transaction;
        }
        catch
        {
            await transaction.DisposeAsync();
            throw;
        }
    }
}
