using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace OnTran.EF.SCU;

public static class DbTransactionExtensions
{
    /// <summary>
    /// Instantiates new scoped instance of EF context (via DI) on current connection and current transaction.
    /// This EF Context will work on connection and transaction from DatabaseFacade, without it's own transaction and connection.
    /// Context inside functor will be the same for all services created with DI (if TDbContext registered as scoped service).
    /// The main purpose of this shortlived-EF-Context is the removing object that has been inserted in DB from memory (because lifetime of DB context is managed and scoped), because EF context holds reference on every inserted object while context is alive. 
    /// * It is not possible to remove an inserted object from memory if the context exists, for example by removing tracking or etc.
    /// * Only objects that are read from the DB can be detached from the context (and then collected by GC), but all inserted objects will remain in it forever because of complicated EF logic of storing PK and metadata of objects.
    /// </summary>
    /// <typeparam name="TDbContext">EF Context DB type</typeparam>
    /// <param name="dbFacade">DatabaseFacade with master connection and master transaction</param>
    /// <param name="serviceScopeFactory">IServiceScopeFactory</param>
    /// <param name="functor">functor with DB insatance(on master transaction and connection) and ServiceProvider realted to current scope</param>
    /// <param name="cancellationToken">token</param>
    public static async Task ConstructScopedContextOnCurrentTranAsync<TDbContext>(this DatabaseFacade? dbFacade, IServiceScopeFactory? serviceScopeFactory, Func<TDbContext, IServiceProvider, Task> functor, CancellationToken cancellationToken = default)
        where TDbContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(dbFacade);
        ArgumentNullException.ThrowIfNull(dbFacade.CurrentTransaction);
        ArgumentNullException.ThrowIfNull(serviceScopeFactory);
        ArgumentNullException.ThrowIfNull(functor);
        await using var scope = serviceScopeFactory.CreateAsyncScope();
        await using var db = scope.ServiceProvider.GetRequiredService<TDbContext>();
        await db.SetTranAndConnAsync(dbFacade.GetDbConnection(), dbFacade.CurrentTransaction!, cancellationToken).ConfigureAwait(false);
        await functor(db, scope.ServiceProvider).ConfigureAwait(false);
    }

    /// <summary>
    /// Instantiates new scoped instance of EF context (via DI) on current connection and current transaction.
    /// This EF Context will work on connection and transaction from DatabaseFacade, without it's own transaction and connection.
    /// Context inside functor will be the same for all services created with DI (if TDbContext registered as scoped service).
    /// The main purpose of this shortlived-EF-Context is the removing object that has been inserted in DB from memory (because lifetime of DB context is managed and scoped), because EF context holds reference on every inserted object while context is alive. 
    /// * It is not possible to remove an inserted object from memory if the context exists, for example by removing tracking or etc.
    /// * Only objects that are read from the DB can be detached from the context (and then collected by GC), but all inserted objects will remain in it forever because of complicated EF logic of storing PK and metadata of objects.
    /// </summary>
    /// <typeparam name="TDbContext">EF Context type</typeparam>
    /// <typeparam name="T">return type</typeparam>
    /// <param name="dbFacade">DatabaseFacade with master connection and master transaction</param>
    /// <param name="serviceScopeFactory">IServiceScopeFactory</param>
    /// <param name="functor">functor with DB insatance(on master transaction and connection) and ServiceProvider realted to current scope</param>
    /// <param name="cancellationToken">token</param>
    /// <returns>T type</returns>
    public static async Task<T> ConstructScopedContextOnCurrentTranAsync<TDbContext, T>(this DatabaseFacade? dbFacade, IServiceScopeFactory? serviceScopeFactory, Func<TDbContext, IServiceProvider, Task<T>> functor, CancellationToken cancellationToken = default)
        where TDbContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(dbFacade);
        ArgumentNullException.ThrowIfNull(dbFacade.CurrentTransaction);
        ArgumentNullException.ThrowIfNull(serviceScopeFactory);
        ArgumentNullException.ThrowIfNull(functor);
        await using var scope = serviceScopeFactory.CreateAsyncScope();
        await using var db = scope.ServiceProvider.GetRequiredService<TDbContext>();
        await db.SetTranAndConnAsync(dbFacade.GetDbConnection(), dbFacade.CurrentTransaction!, cancellationToken).ConfigureAwait(false);
        return await functor(db, scope.ServiceProvider).ConfigureAwait(false);
    }
}
