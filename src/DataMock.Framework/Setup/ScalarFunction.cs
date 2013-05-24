using System;

namespace LazyE9.DataMock.Setup
{
    internal class ScalarFunction : Function
    {
        #region Constructors

        public ScalarFunction(string name, MethodParam[] parameters, Type returnType)
            : base(name, parameters)
        {
            returnType = Nullable.GetUnderlyingType(returnType) ?? returnType;
            mReturnType = returnType.Name;

            //temporary work around until we can figure out how to convert Type objects into the appropriate DBType using the correct provider
            mReturnType = "sql_variant";
        }

        #endregion Constructors

        #region Fields

        private readonly string mReturnType;

        #endregion Fields

        #region Protected Members

        protected override string CreateStatementFormat()
        {

            return "CREATE FUNCTION {0}\n(\n{1}\n)\nRETURNS " + mReturnType + " AS \nBEGIN\nRETURN (SELECT TOP 1 RESULT FROM \n(\n{2}\n) AS CANDIDATES\n)\nEND";
        }

        protected override string[] GetSqlObjectTypes()
        {
            return new[] { "FN", "IF", "TF", "FS", "FT" };
        }

        #endregion Protected Members

    }
}