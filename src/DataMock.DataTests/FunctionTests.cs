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

//        [TestMethod]
//        public void MockFunctionWithConditionalResults()
//        {
//            var dataMock = new DataMock<DataMockDataContext>
//            {
//                Log = Console.Out
//            };
//
//            var queryGuid = Guid.NewGuid();
//            var queryDate = DateTime.Today;
//
//            var resultGuid = Guid.Empty;
//            var resultDate = new DateTime(2012, 1, 1);
//
//            dataMock
//                .Setup(context => context.DataTypesFunction(Param.Is<int>(v => v == 1), Param.Is<string>(v => v == "1"), Param.IsAny<Guid>(), Param.IsAny<DateTime>()))
//                .Returns(new DataTypesFunctionResult
//                {
//                    c1 = 1,
//                    c2 = "1",
//                    c3 = resultGuid,
//                    c4 = resultDate
//                });
//
//            dataMock
//               .Setup(context => context.DataTypesFunction(Param.Is<int>(v => v == 2), Param.Is<string>(v => v == "2"), Param.IsAny<Guid>(), Param.IsAny<DateTime>()))
//               .Returns(new DataTypesFunctionResult
//               {
//                   c1 = 2,
//                   c2 = "2",
//                   c3 = resultGuid,
//                   c4 = resultDate
//               });
//
//            dataMock
//               .Setup(context => context.DataTypesFunction(Param.IsAny<int>(), Param.IsAny<string>(), Param.IsAny<Guid>(), Param.IsAny<DateTime>()))
//               .Returns(new DataTypesFunctionResult
//               {
//                   c1 = 3,
//                   c2 = "3",
//                   c3 = resultGuid,
//                   c4 = resultDate
//               });
//            dataMock.Execute(Settings.Default.DataMockConnectionString);
//
//            using (var context = new DataMockDataContext(Settings.Default.DataMockConnectionString))
//            {
//                DataTypesFunctionResult one = context.DataTypesFunction(1, "1", queryGuid, queryDate).Single();
//                DataTypesFunctionResult two = context.DataTypesFunction(2, "2", queryGuid, queryDate).Single();
//                DataTypesFunctionResult otherOne = context.DataTypesFunction(1, "hat", queryGuid, queryDate).Single();
//                DataTypesFunctionResult otherTwo = context.DataTypesFunction(3, "3", queryGuid, queryDate).Single();
//
//                Assert.AreEqual(1, one.c1);
//                Assert.AreEqual("1", one.c2);
//
//                Assert.AreEqual(2, two.c1);
//                Assert.AreEqual("2", two.c2);
//
//                Assert.AreEqual(3, otherOne.c1);
//                Assert.AreEqual("3", otherOne.c2);
//
//                Assert.AreEqual(3, otherTwo.c1);
//                Assert.AreEqual("3", otherTwo.c2);
//            }
//        }

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

            const int RESULT_INT = 1;
            const string RESULT_STRING = "12345";
            var resultGuid = Guid.Empty;
            var resultDate = new DateTime(2012, 1, 1);

            dataMock
                .Setup(context => context.DataTypesFunction(QUERY_INT, QUERY_STRING, queryGuid, queryDate))
                .Returns(new DataTypesFunctionResult
                {
                    c1 = RESULT_INT,
                    c2 = RESULT_STRING,
                    c3 = resultGuid,
                    c4 = resultDate
                });
            dataMock.Execute(Settings.Default.DataMockConnectionString);

            using (var context = new DataMockDataContext(Settings.Default.DataMockConnectionString))
            {
                DataTypesFunctionResult result = context.DataTypesFunction(QUERY_INT, QUERY_STRING, queryGuid, queryDate).Single();
                Assert.AreEqual(RESULT_INT, result.c1);
                Assert.AreEqual(RESULT_STRING, result.c2);
                Assert.AreEqual(resultGuid, result.c3);
                Assert.AreEqual(resultDate, result.c4);
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
                .Setup(context => context.DataTypesFunction(QUERY_INT, QUERY_STRING, new Guid("F255039E-7809-E211-BDD9-08002704F29D"), DateTime.Today))
                .Returns(new DataTypesFunctionResult
                {
                    c1 = RESULT_INT,
                    c2 = RESULT_STRING,
                    c3 = resultGuid
                });
            dataMock.Execute(Settings.Default.DataMockConnectionString);

            using (var context = new DataMockDataContext(Settings.Default.DataMockConnectionString))
            {
                DataTypesFunctionResult result = context.DataTypesFunction(QUERY_INT, QUERY_STRING, queryGuid, DateTime.Today).Single();
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

//        [TestMethod]
//        public void MockScalarFunctionReturnsDependantValue()
//        {
//            var dataMock = new DataMock<DataMockDataContext>
//            {
//                Log = Console.Out
//            };
//
//            Expression<Func<int?, bool>> lessThanOrEqualToTen = v => v <= 10;
//            dataMock
//                .Setup(context => context.ScalarFunction(Param.Is(lessThanOrEqualToTen), Param.Is(lessThanOrEqualToTen)))
//                .Returns(1);
//
//            Expression<Func<int?, bool>> greaterThanTen = v => v > 10;
//            dataMock
//                .Setup(context => context.ScalarFunction(Param.Is(greaterThanTen), Param.Is(greaterThanTen)))
//                .Returns(2);
//
//            dataMock.Execute(Settings.Default.DataMockConnectionString);
//
//            using (var context = new DataMockDataContext(Settings.Default.DataMockConnectionString))
//            {
//                Assert.AreEqual(1, context.ScalarFunction(1, 1));
//                Assert.AreEqual(1, context.ScalarFunction(1, 10));
//                Assert.AreEqual(1, context.ScalarFunction(10, 10));
//                Assert.AreEqual(1, context.ScalarFunction(10, 10));
//                Assert.AreEqual(2, context.ScalarFunction(11, 11));
//            }
//        }

        #endregion FunctionTests Members

    }
}
