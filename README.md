# [OnTran.EF.SCU](https://github.com/sapozhnikovv/OnTran.EF.SCU)
![Logo](https://github.com/sapozhnikovv/OnTran.EF.SCU/blob/main/img/ontran.ef.png)
 
Use your EF-based code without any changes, without raw SQL, without breaking the transactionality and without application crashes due to OutOfMemory errors.   
Minimal, Effective, multi-target EF Core extension for run short-lived contexts on the same connection and transaction when micro-ORM (like Linq2db) cannot be used (and you cannot use batch data insertion for some reason).   
Short-lived contexts will be used as the SQL formatter, without storing all inserted objects in the application memory due to the short lifetime of the scoped context.   

The main purpose of this extension is Memory Management. By using short-lived contexts, inserted entities can be garbage collected, preventing memory leaks.

**To save memory, you need to call this extension several times to insert a large number of objects; If you need to insert 1_000_000 objects, then you can call this extension (in a loop) for each 1_000 - 10_000 pieces**   

## DI-Friendly Integration   
Services resolved from the dependency injection scope within the functor automatically use the shared main transaction and connection, without any code changes.

## Key Difference from TransactionScope   
Unlike TransactionScope (which uses ambient transactions with the limitations on Linux/container), this extension:   
- Uses explicit connection/transaction sharing (no MSDTC dependency, no distributed transaction coordinator)   
- Works natively in Docker/Linux containers   
- Creates scoped short-lived contexts that integrate with DI   

All services from the dependency injection scope automatically use the shared transaction and connection.    

In essence, it's not just about 'setting connection and transaction for context' - it's about creating scoped short-lived contexts for services that use the local transaction, with a primary focus on memory management.


Support DB types:   
✅ MySql-based   
✅ Postgres-based   
✅ MS Sql   

> **Note**: While this extension should work with any EF Core provider, testing has only been performed on these three databases.

[Examples-Tests](https://github.com/sapozhnikovv/OnTran.EF.SCU/tree/main/Examples-Tests)
These tests are run on the .NET8.0 using TestContainers to run the DB's in Docker.

If the functionality of this solution does not meet your needs, feel free to make your own version of this extension or just copy-paste code in your solution and change. It is open source.


# Nuget
multi-target package:   
✅ .net6.0   
✅ .net7.0   
✅ .net8.0   
✅ .net9.0   

https://www.nuget.org/packages/OnTran.EF.SCU

```shell
dotnet add package OnTran.EF.SCU
```
or
```shell
NuGet\Install-Package OnTran.EF.SCU
```

## Why Use This Extension?

| Scenario | Problem | Solution |
|----------|---------|----------|
| **Batch Insertions** | EF Context tracks all inserted entities, causing memory bloat | ✅ Short-lived contexts allow garbage collection |
| **Microservices in Containers** | TransactionScope has limited Linux/Docker support | ✅ No MSDTC, works natively in containers |
| **Complex Business Logic** | Need multiple DbContexts in one transaction without code duplication | ✅ Shared transaction with DI integration |

## Example of using

```c#
using OnTran.EF.SCU;

await using var tran = await context.Database.BeginTransactionAsync(token);
//Get IServiceScopeFactory from DI to use this extension (via constructor of your class or via ServiceProvider).
var affected = await context.Database.ConstructScopedContextOnCurrentTranAsync<DBContext, int>(serviceScopeFactory, async (db, scope) =>
{
	// The 'db' instance from the functor is not equal to the 'context' instance. 'db' is a context instance without its own connection and transaction, it uses conn/tran from 'context'.
    for (var i = 0; i < 1000; i++)
	{
		db.TestEntities.Add(new TestEntity { Value = i.ToString() });
	}
    return await db.SaveChangesAsync(token);
}, token);
//all TestEntity will be gone (not immediately) from memory because 'db' context from the method above is disposed.
await context.Database.ConstructScopedContextOnCurrentTranAsync<DBContext>(serviceScopeFactory, async (db, scope) =>
{
    for (var i = 0; i < 1000; i++)
	{
		db.TestEntities.Add(new TestEntity { Value = i.ToString() });
	}
    await db.SaveChangesAsync(token);
}, token);
//all TestEntity will be gone (not immediately) from memory because 'db' context from the method above is disposed.
await context.Database.ConstructScopedContextOnCurrentTranAsync<DBContext>(serviceScopeFactory, async (db, scope) =>
{
    await scope.GetService<MyService>().ProcessManyRecordsAndInsertManyRecordsWithComplicatedLogicAsync(token);
	//The DbContext instance injected into MyService from 'scope' without own connection and transaction, it is 'db' from functor, it uses the main transaction/connection from 'context'.
	//So, you can use any services via DI without code changes.
}, token);
//all TestEntity will be gone (not immediately) from memory because 'db' context from the method above is disposed.
await tran.CommitAsync(token);
```

## License
Free MIT license (https://github.com/sapozhnikovv/OnTran.EF.SCU/blob/main/LICENSE)

