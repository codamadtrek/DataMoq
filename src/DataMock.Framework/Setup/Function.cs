namespace LazyE9.DataMock.Setup
{
    internal abstract class Function : CallableDatabaseObject
    {
        #region Constructors

        protected Function(string name, MethodParam[] parameters)
            : base(name, parameters)
        {
        }

        #endregion Constructors

        #region Protected Members

        protected override string DataObjectTypeName
        {
            get { return "FUNCTION"; }
        }

        #endregion Protected Members

    }
}