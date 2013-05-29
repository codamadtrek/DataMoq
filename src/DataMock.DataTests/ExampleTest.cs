using System;
using System.Data.Linq;
using System.Linq;
using DataMock.DataTests.DataObjects;
using DataMock.DataTests.Properties;
using LazyE9.DataMock;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace DataMock.DataTests
{
    [TestClass]
    public class FinalizeOrderTests : TestBase
    {
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
    }
}
