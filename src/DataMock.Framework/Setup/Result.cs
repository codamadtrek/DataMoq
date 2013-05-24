using System;

namespace LazyE9.DataMock.Setup
{
    internal class Result
    {
        #region Constructors

        public Result(object result)
        {
            ResultData = result;
        }

        #endregion Constructors

        #region Result Members

        public virtual string GetSelectStatement()
        {
            return DataMockHelper.ResultToSelectStatement(ResultData, String.Empty);
        }

        #endregion Result Members

        #region Internal Members

        internal object ResultData
        {
            get;
            private set;
        }

        #endregion Internal Members

    }
}