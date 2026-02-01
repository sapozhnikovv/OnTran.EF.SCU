using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data.Common;

namespace OnTran.EF.SCU;

internal static class DbContextExtensions
{
    /// <summary>
    /// Swap\Set connection and transaction for EF context
    /// </summary>
    internal static async Task SetTranAndConnAsync<TDbContext>(this TDbContext context, DbConnection connection, IDbContextTransaction transaction, CancellationToken cancellationToken = default)
        where TDbContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(transaction);
        await context.Database.CloseConnectionAsync().ConfigureAwait(false);
        context.Database.SetDbConnection(connection);
        await context.Database.UseTransactionAsync(transaction.GetDbTransaction(), cancellationToken).ConfigureAwait(false);
    }
}
