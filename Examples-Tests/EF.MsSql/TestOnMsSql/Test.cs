using EF.MsSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OnTran.EF.SCU;
using Shouldly;

namespace TestOnMsSql;


public class Test(DatabaseAndAppFixture appFixture) : IClassFixture<DatabaseAndAppFixture>
{
    /// <summary>
    /// it will be collected by GC
    /// </summary>
    public Task<(WeakReference @ref, int affected)> InsertManyRecordsInScopedContextOnCurrentTranAndMarkLastObjectInRAMAsync(IServiceScopeFactory serviceScopeFactory, string value, int count) =>
        appFixture.DB.Database.ConstructScopedContextOnCurrentTranAsync<MsSqlContext, (WeakReference, int)>(serviceScopeFactory, async (db, scope) =>
        {
            for (var i = 0; i < count; i++)
            {
                db.TestEntities.Add(new TestEntity { Value = i.ToString() });
            }
            var val = new TestEntity { Value = value };
            db.TestEntities.Add(val);
            var @ref = new WeakReference(val);
            return (@ref, await db.SaveChangesAsync());
        });

    /// <summary>
    /// it will not be collected by GC
    /// </summary>
    private async Task<WeakReference> InsertAndDetachAsync(string value)
    {
        var val = new TestEntity { Value = value };
        appFixture.DB.TestEntities.Add(val);
        var @ref = new WeakReference(val);
        await appFixture.DB.SaveChangesAsync();

        foreach (var entry in appFixture.DB.ChangeTracker.Entries<TestEntity>()) entry.State = EntityState.Detached;
        appFixture.DB.ChangeTracker.Clear();

        return @ref;
    }

    [Fact]
    public async Task TestMsSql()
    {
        //Arrange
        var serviceScopeFactory = appFixture.Scope.ServiceProvider.GetRequiredService<IServiceScopeFactory>();
        const string firstVal = "F", secondVal = "S", lastVal = "L";
        const int countValues = 1_000;

        //Act
        await using var tran = await appFixture.DB.Database.BeginTransactionAsync(System.Data.IsolationLevel.Snapshot);

        var diagnostics = await appFixture.DB.Database.SqlQueryRaw<string>("""  
        SELECT CONCAT(
          'SNAPSHOT_DB=', CASE (SELECT snapshot_isolation_state FROM sys.databases WHERE name = DB_NAME()) WHEN 1 THEN 'ON' ELSE 'OFF' END, 
          '|ISOLATION=', (SELECT transaction_isolation_level FROM sys.dm_exec_sessions WHERE session_id = @@SPID)
        ) as 'Value'
        """).FirstOrDefaultAsync();

        var (firstRef, affected1) = await InsertManyRecordsInScopedContextOnCurrentTranAndMarkLastObjectInRAMAsync(serviceScopeFactory, firstVal, countValues);

        var count1 = await appFixture.DB.TestEntities.CountAsync();//it can read data from main transaction

        var (secondRef, affected2) = await InsertManyRecordsInScopedContextOnCurrentTranAndMarkLastObjectInRAMAsync(serviceScopeFactory, secondVal, countValues);

        var count2 = await appFixture.DB.TestEntities.CountAsync();//it can read data from main transaction

        await using var checkScope = serviceScopeFactory.CreateAsyncScope();
        var checkContext = checkScope.ServiceProvider.GetRequiredService<MsSqlContext>();
        await using var checkTran = await checkContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.Snapshot);
        var visibleCount1 = await checkContext.TestEntities.CountAsync();//it cannot read data from main transaction

        await tran.CommitAsync();
        await checkTran.CommitAsync();

        var count = await appFixture.DB.TestEntities.CountAsync();
        var visibleCount2 = await checkContext.TestEntities.CountAsync();

        var lastRef = await InsertAndDetachAsync(lastVal);

        var first = await appFixture.DB.TestEntities.Where(t => t.Value == firstVal).FirstOrDefaultAsync();
        var second = await appFixture.DB.TestEntities.Where(t => t.Value == secondVal).FirstOrDefaultAsync();

        //Assert

        diagnostics.ShouldBe("SNAPSHOT_DB=ON|ISOLATION=5");
        //only in SNAPSHOT transactions we can read data when another tran changed\locked data in table (we need to check 0 records from another tran, before commit of main tran)

        affected1.ShouldBe(countValues+1);
        affected2.ShouldBe(countValues+1);
        count1.ShouldBe(countValues+1);
        count2.ShouldBe((countValues+1)*2);

        count.ShouldBe(affected1 + affected2);
        visibleCount2.ShouldBe(count);

        visibleCount1.ShouldBe(0, "we cannot see records in table in another transaction");

        first.ShouldNotBeNull();
        first.ShouldNotBeNull();

        GC.Collect();
        GC.WaitForPendingFinalizers();

        firstRef!.IsAlive.ShouldBeFalse();
        secondRef!.IsAlive.ShouldBeFalse();
        lastRef!.IsAlive.ShouldBeTrue();
    }

}