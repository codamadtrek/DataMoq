using System;
using System.Data.SqlClient;

using DataMock.DataTests.DataObjects;
using DataMock.DataTests.Properties;

using LazyE9.DataMock;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataMock.DataTests
{
    [TestClass]
    public class RemoveMockTests : TestBase
    {
        #region RemoveMockTests Members

        [TestMethod]
        public void RemoveFunctionMock()
        {
            _DoMockRemovalTest("ScalarFunction", dataMock => dataMock
                .Setup(context => context.ScalarFunction(Param.IsAny<int?>(), Param.IsAny<int?>()))
                .Returns(0));
        }

        [TestMethod]
        public void RemoveSprocMock()
        {
            _DoMockRemovalTest("VariableOutputProcA", dataMock => dataMock
                .Setup(context => context.VariableOutputProcA(Param.IsAny<int?>()))
                .Returns(new VariableOutputProcAResult()));
        }

        [TestMethod]
        public void RemoveViewMock()
        {
            _DoMockRemovalTest("People", dataMock => dataMock
                .Setup(context => context.Peoples)
                .Returns(new People()));
        }

        #endregion RemoveMockTests Members

        #region Private Members

        private void _DoMockRemovalTest(string objectName, Action<DataMock<DataMockDataContext>> setupMock)
        {
            var dataMock = new DataMock<DataMockDataContext>
            {
                Log = Console.Out
            };

            setupMock(dataMock);

            string dataMockConnectionString = Settings.Default.DataMockConnectionString;
            dataMock.Execute(dataMockConnectionString);
            dataMock.RemoveMocks(dataMockConnectionString);

            using (var connection = new SqlConnection(dataMockConnectionString))
            {
                connection.Open();
                SqlCommand command = connection.CreateCommand();

                command.CommandText =
                    @"select 
	                    1 
                    WHERE NOT EXISTS ( SELECT NULL FROM sys.objects WHERE Name = @ObjectName )";

                command.Parameters.AddWithValue("@ObjectName", objectName);
                
                object result = command.ExecuteScalar();

                Assert.AreEqual(result, 1);
            }

        }

        #endregion Private Members

    }
}
