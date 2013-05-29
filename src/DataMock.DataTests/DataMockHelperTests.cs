using System;

using DataMock.DataTests.DataObjects;

using LazyE9.DataMock;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataMock.DataTests
{
    [TestClass]
    public class DataMockHelperTests : TestBase
    {
        #region DataMockHelperTests Members

        [TestMethod]
        public void DropConstraintBasic()
        {
            try
            {
                _RunInsert();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }

            using (var context = new DataMockDataContext())
            {
                context.DisableAllCheckConstraints<DataMockDataContext, Child>();
            }

            _RunInsert("Ralph");
        }

        [TestMethod]
        public void FormatNonNullNullableChar()
        {
            char? value = '1';
            string result = DataMockHelper.FormatValue(value);
            Assert.AreEqual("'1'", result);
        }

        [TestMethod]
        public void FormatNonNullNullableInt()
        {
            int? value = 1;
            string result = DataMockHelper.FormatValue(value);
            Assert.AreEqual("1", result);
        }

        [TestMethod]
        public void FormatNullNullableChar()
        {
            char? value = null;
            string result = DataMockHelper.FormatValue(value);
            Assert.AreEqual("NULL", result);
        }

        [TestMethod]
        public void FormatNullNullableInt()
        {
            int? value = null;
            string result = DataMockHelper.FormatValue(value);
            Assert.AreEqual("NULL", result);
        }

        #endregion DataMockHelperTests Members

        #region Private Members

        private void _RunInsert(string name = null)
        {
            using (var context = new DataMockDataContext())
            {
                context.Parents.InsertOnSubmit(new Parent
                {
                    Name = name ?? string.Empty
                });
                context.SubmitChanges();
            }
        }

        #endregion Private Members

    }
}
