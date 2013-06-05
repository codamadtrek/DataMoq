# DataMock

DataMock helps you unit test complex SQL code by allowing you to mock SQL tables, views, functions, and procedures.
Mocks are specified in code using a a fluent API over Linq 2 SQL. At runtime mocks are created in the database by droping 
the existing object, and creating a new object with the same name that satisfies the constraints of the mock. 

DataMock can be used with any unit testing framework.

## Basic Usage

1. Reference the LazyE9.DataMock.Framework assembly in your test project.

2. Create a LinqToSQL data context to reference the tables, views, functions, and procedures that you want to unit test, or included as dependencies in your tests.

3. Choose a method of rolling back test transactions, (discussed below)

4. Create a new DataMock object for your LinqToSQL data context type.

5. Setup individual mocks for the context using the .Setup() method.

6. Call the Execute() method on the DataMock object to create your mocks in the database.

7. Test the database object.

8. Roll back the transaction to prevent your mocks from permanently replacing the database objects they are mocking.

## Example

Consider the following SQL procedure:

```sql
CREATE PROCEDURE [dbo].[FinalizeOrder]
	@orderId int = 0
AS
	DECLARE @sequence INT = dbo.[GetNextOrderSequence]()

	UPDATE [Order]	
	SET
		OrderSequenceNumber = @sequence,
		OrderDate = GETDATE()
	OUTPUT @sequence AS Sequence
	WHERE OrderId = @orderId
RETURN
```

It is difficult to unit test this procedure without relying on implementation of GetNextOrderSequenceNumber(). Using DataMock,
we can mock this function so that it returns a known value. This makes it much easier to write a simple unit test.

This test mocks the GetNextOrderSequence function so that it returns a known value. It then calls the FinalizeOrder procedure
for an existing order, and verifies that the correct sequence is returned, and saved in the OrderSequenceNumber column of the
correct Order row.

```csharp
[TestMethod]
public void FinalizeExistingOrder()
{
    var dataMock = new DataMock<DataMockDataContext>
    {
        Log = Console.Out
    };

    const int NEXT_SEQUENCE = 3;
    dataMock
        .Setup( context => context.GetNextOrderSequence())
        .Returns(NEXT_SEQUENCE);

    using (var context = new DataMockDataContext(Settings.Default.DataMockConnectionString))
    {
        //Disable all check constraints on the Order table to simplify inserting test data
        context.DisableAllCheckConstraints<DataMockDataContext, Order>();

        //Create an Order object, and set its properties to sensible default values
        var orderToUpdate = new Order();
        DataMockHelper.DefaultValues(orderToUpdate);

        //add the order to the database
        context.Orders.InsertOnSubmit(orderToUpdate);
        context.SubmitChanges();

        //create the mocked implementation of GetNextOrderSequence()
        dataMock.Execute(Settings.Default.DataMockConnectionString);

        //execute the FinalizeOrder procedure and verify its results
        FinalizeOrderResult finalizeOrderResult = context.FinalizeOrder(orderToUpdate.OrderId).SingleOrDefault();

        Assert.IsNotNull(finalizeOrderResult);
        Assert.AreEqual(NEXT_SEQUENCE, finalizeOrderResult.Sequence);

        //verify that the procedure updated the order table
        context.Refresh(RefreshMode.OverwriteCurrentValues, orderToUpdate);
        Assert.AreEqual(NEXT_SEQUENCE, orderToUpdate.OrderSequenceNumber);
    }
}
```

## Parameters

Parameters for functions or procedures are implemented as where clauses within the body of the mocked function or procedure. 
For an exact parameter match, simply specify the parameter values directly:

```csharp
dataMock
    .Setup(context => context.DataTypesFunction(10, "TEST", new Guid("35598247-9a10-4782-b810-c28045cc3d16"), new Date(2013, 05, 29)))
    .Returns(new DataTypesFunctionResult
    {
        c1 = 1,
        c2 = "1234",
        c3 = Guid.Empty,
        c4 = new DateTime(2012, 1, 1)
    });
```

is implemented in the database as:

```sql
CREATE FUNCTION dbo.DataTypesFunction
(
	@int Int
	,@string Char(5)
	,@guid UniqueIdentifier
	,@dateTime DateTime
)
RETURNS TABLE AS RETURN
(
	(SELECT CAST( 1 AS Int) AS c1, CAST( '12345' AS Char(5)) AS c2, CAST( '00000000-0000-0000-0000-000000000000' AS UniqueIdentifier) AS c3, CAST( '1/1/2012 12:00:00 AM' AS DateTime) AS c4 
	WHERE  
		@int = CAST(10 AS Int) 
		AND  @string = CAST('TEST' AS Char(5)) 
		AND  @guid = CAST('35598247-9a10-4782-b810-c28045cc3d16' AS UniqueIdentifier) 
		AND  @dateTime = CAST('2013-05-29 00:00:00' AS DateTime))
)
```

If you want your function or procedure to return a certain result for any parameter value, use the Param.Any<>() syntax. This will produce a mock without a where clause.

```csharp
dataMock
    .Setup(context => context.DataTypesFunction(Param.IsAny<int?>, Param.IsAny<string>, Param.IsAny<Guid>, Param.IsAny<DateTime>))
    .Returns(new DataTypesFunctionResult
    {
        c1 = 1,
        c2 = "1234",
        c3 = Guid.Empty,
        c4 = new DateTime(2012, 1, 1)
    });
```

is implemented in the database as:

```sql
CREATE FUNCTION dbo.DataTypesFunction
(
	@int Int
	,@string Char(5)
	,@guid UniqueIdentifier
	,@dateTime DateTime
)
RETURNS TABLE AS RETURN
(
	(SELECT CAST( 1 AS Int) AS c1, CAST( '12345' AS Char(5)) AS c2, CAST( '00000000-0000-0000-0000-000000000000' AS UniqueIdentifier) AS c3, CAST( '1/1/2012 12:00:00 AM' AS DateTime) AS c4)
)
```

## Debugging

To see the SQL that DataMock uses to create your mocks set the Log property to an instance of the TextWriter class.

```csharp
var dataMock = new DataMock<DataMockDataContext>
{
    Log = Console.Out
};
```

## Transactions

To prevent your mocks from remaining permanent, you need to run your unit tests inside transactions. If your tests only connect to one database, then you can create your transactions using the TransactionScope object:

```csharp
using( new TransactionScope( TransactionScopeOption.RequiresNew, TimeSpan.FromMinutes( 2 ) ) )
{
	dataMock.Execute( ConnectionString );

	//test code goes here
} //this transaction will be rolled back becasue there is no call to it's Commit() method.
```

It may be convenient to create a common base class for your database unit tests. This base class can manage the transaction for each test. How you do this depends on the unit test framework you are using.

```csharp
[TestClass]
public class TestBase
{
	#region TestBase Members

	[TestCleanup]
	public void CleanupTestTransaction()
	{
		if( mCurrentTestTransaction != null )
		{
			mCurrentTestTransaction.Dispose();
			mCurrentTestTransaction = null;
		}
	}

	[TestInitialize]
	public void InitializeTestTransaction()
	{
		mCurrentTestTransaction = new TransactionScope( TransactionScopeOption.RequiresNew, TimeSpan.FromMinutes( 2 ) );
	}

	#endregion TestBase Members

	#region Fields

	private TransactionScope mCurrentTestTransaction;

	#endregion Fields

}
```

If your tests connect to multiple databases, you will need to use use something like XtUnit to create distributed transactions.
