namespace LazyE9.DataMock.Language
{
    internal class ParameterParseResult
    {
        #region Constructors

        public ParameterParseResult(object value, string sqlValue, bool exactParameterSet = true)
        {
            Value = value;
            ExactParameterSet = exactParameterSet;
            SqlValue = sqlValue;
        }

        #endregion Constructors

        #region ParameterParseResult Members

        public bool ExactParameterSet
        {
            get;
            private set;
        }

        public string SqlValue { get; private set; }

        public object Value
        {
            get;
            private set;
        }

        #endregion ParameterParseResult Members

    }
}