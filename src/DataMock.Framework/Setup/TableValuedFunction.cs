namespace LazyE9.DataMock.Setup
{
    internal class TableValuedFunction : Function
	{
		#region Constructors

		public TableValuedFunction( string name, MethodParam[] parameters )
			: base( name, parameters )
		{
		}

		#endregion Constructors

		#region Protected Members

		protected override string CreateStatementFormat()
		{
			return "CREATE FUNCTION {0}\n(\n{1}\n)\nRETURNS TABLE AS RETURN\n(\n{2}\n)";
		}

		protected override string[] GetSqlObjectTypes()
		{
			return new[] { "FN", "IF", "TF", "FS", "FT" };
		}

		#endregion Protected Members

	}
}