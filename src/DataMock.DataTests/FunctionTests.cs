using System;
using System.Linq;

using DataMock.DataTests.DataObjects;
using DataMock.DataTests.Properties;

using LazyE9.DataMock;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataMock.DataTests
{
    [TestClass]
    public class FunctionTests : TestBase
    {
        #region FunctionTests Members

        [TestMethod]
        public void MockBooleanScalarFunction()
        {
            var dataMock = new DataMock<DataMockDataContext>
            {
                Log = Console.Out
            };

            using (var context = new DataMockDataContext(Settings.Default.DataMockConnectionString))
            {
                Assert.AreEqual(true, context.ScalarFunctionBoolean(1));

                dataMock
                    .Setup(ctx => ctx.ScalarFunctionBoolean(Param.IsAny<int?>()))
                    .Returns(false);

                dataMock.Execute(Settings.Default.DataMockConnectionString);

                Assert.AreEqual(false, context.ScalarFunctionBoolean(1));
            }
        }

        [TestMethod]
        public void MockFunctionWithExactParameters()
        {
            var dataMock = new DataMock<DataMockDataContext>
            {
                Log = Console.Out
            };

            const int QUERY_INT = 10;
            const string QUERY_STRING = "TEST";
            var queryGuid = Guid.NewGuid();
            var queryDate = DateTime.Today;
            const bool QUERY_BIT = true;

            const int RESULT_INT = 1;
            const string RESULT_STRING = "12345";
            var resultGuid = Guid.Empty;
            var resultDate = new DateTime(2012, 1, 1);
            const bool RESULT_BIT = true;


            dataMock
                .Setup(context => context.DataTypesFunction(QUERY_INT, QUERY_STRING, queryGuid, queryDate, QUERY_BIT))
                .Returns(new DataTypesFunctionResult
                {
                    c1 = RESULT_INT,
                    c2 = RESULT_STRING,
                    c3 = resultGuid,
                    c4 = resultDate,
                    c5 = RESULT_BIT
                });
            dataMock.Execute(Settings.Default.DataMockConnectionString);

            using (var context = new DataMockDataContext(Settings.Default.DataMockConnectionString))
            {
                DataTypesFunctionResult result = context.DataTypesFunction(QUERY_INT, QUERY_STRING, queryGuid, queryDate, QUERY_BIT).Single();
                Assert.AreEqual(RESULT_INT, result.c1);
                Assert.AreEqual(RESULT_STRING, result.c2);
                Assert.AreEqual(resultGuid, result.c3);
                Assert.AreEqual(resultDate, result.c4);
                Assert.AreEqual(RESULT_BIT, result.c5);
            }
        }

        [TestMethod]
        public void MockFunctionWithNewGuidExpression()
        {
            var dataMock = new DataMock<DataMockDataContext>
            {
                Log = Console.Out
            };

            const int QUERY_INT = 10;
            const string QUERY_STRING = "TEST";
            var queryGuid = new Guid("F255039E-7809-E211-BDD9-08002704F29D");

            const int RESULT_INT = 1;
            const string RESULT_STRING = "12345";
            var resultGuid = Guid.Empty;

            dataMock
                .Setup(context => context.DataTypesFunction(QUERY_INT, QUERY_STRING, new Guid("F255039E-7809-E211-BDD9-08002704F29D"), DateTime.Today, Param.IsAny<bool>()))
                .Returns(new DataTypesFunctionResult
                {
                    c1 = RESULT_INT,
                    c2 = RESULT_STRING,
                    c3 = resultGuid
                });
            dataMock.Execute(Settings.Default.DataMockConnectionString);

            using (var context = new DataMockDataContext(Settings.Default.DataMockConnectionString))
            {
                DataTypesFunctionResult result = context.DataTypesFunction(QUERY_INT, QUERY_STRING, queryGuid, DateTime.Today, true).Single();
                Assert.AreEqual(RESULT_INT, result.c1);
                Assert.AreEqual(RESULT_STRING, result.c2);
                Assert.AreEqual(resultGuid, result.c3);
            }
        }

        [TestMethod]
        public void MockScalarFunctionReturnsConstantValue()
        {
            var dataMock = new DataMock<DataMockDataContext>
            {
                Log = Console.Out
            };

            dataMock
                .Setup(context => context.ScalarFunction(Param.IsAny<int?>(), Param.IsAny<int?>()))
                .Returns(1);

            dataMock.Execute(Settings.Default.DataMockConnectionString);

            using (var context = new DataMockDataContext(Settings.Default.DataMockConnectionString))
            {
                int? result = context.ScalarFunction(1, 1);
                Assert.AreEqual(result, 1);
            }
        }

        [TestMethod]
        public void MockStringScalarFunction()
        {
            var dataMock = new DataMock<DataMockDataContext>
            {
                Log = Console.Out
            };

            using (var context = new DataMockDataContext(Settings.Default.DataMockConnectionString))
            {
                Assert.AreEqual(true, context.ScalarFunctionBoolean(1));

                dataMock
                    .Setup(ctx => ctx.ScalarFunctionString(Param.IsAny<int?>()))
                    .Returns("Mocked");

                dataMock.Execute(Settings.Default.DataMockConnectionString);

                Assert.AreEqual("Mocked", context.ScalarFunctionString(1));
            }
        }

        #endregion FunctionTests Members

    }
}
