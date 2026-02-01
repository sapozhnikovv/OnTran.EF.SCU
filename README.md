# [OnTran.EF.SCU](https://github.com/sapozhnikovv/OnTran.EF.SCU)
![Logo](https://github.com/sapozhnikovv/OnTran.EF.SCU/blob/main/img/ontranef.png)

Use your EF-based code without any changes, without raw SQL, without breaking the transactionality and without application crashes due to OutOfMemory errors.   
Minimal, Effective, multi-target EF Core extension for run short-lived contexts on the same connection and transaction when micro-ORM cannot be used.   
Short-lived contexts will be used as the SQL formatter, without storing all inserted objects in the application memory due to the short lifetime of the scoped context.   

## DI-Friendly Integration   
Services resolved from the dependency injection scope within the functor automatically use the shared main transaction and connection, without any code changes.

## Key Difference from TransactionScope   
Unlike TransactionScope (which uses ambient transactions with the limitations for Linux/container), this extension:   
- Uses explicit connection/transaction sharing (no MSDTC dependency, no distributed transaction coordinator)   
- Works natively in Docker/Linux containers   
- Creates scoped short-lived contexts that integrate with DI   

All services from the dependency injection scope automatically use the shared transaction and connection.    

So, It is not only about 'set connection and transaction for context', it also about 'create scoped short-lived context for services from scope' and use local transaction.   


Support DB types:   
✅ MySql-based   
✅ Postgres-based   
✅ MS Sql   

* Other database types can be used, but testing was only done on these three.

[Examples-Tests](https://github.com/sapozhnikovv/OnTran.EF.SCU/tree/main/Examples-Tests)
These tests on .net8.0 and use TestContainers to run DB in Docker. 

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
	//DB context instance from DI in MyService instance from scope.GetService<MyService>() will use not own transaction/connetion, but you main tran/conn.
	//So, you can use any services via DI without code changes.
}, token);
//all TestEntity will be gone (not immediately) from memory because 'db' context from the method above is disposed.
await tran.CommitAsync(token);
```

## License
Free MIT license (https://github.com/sapozhnikovv/OnTran.EF.SCU/blob/main/LICENSE)

