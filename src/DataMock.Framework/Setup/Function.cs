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

        #region Internal Members

        protected internal override string DataObjectTypeName
        {
            get { return "FUNCTION"; }
        }

        protected internal override string[] SqlObjectTypes
        {
            get { return new[] { "FN", "IF", "TF", "FS", "FT" }; }
        }

        #endregion Internal Members

    }
}