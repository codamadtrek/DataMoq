namespace LazyE9.DataMock.Setup
{
	internal class Procedure : CallableDatabaseObject
	{
		#region Constructors

		public Procedure( string name, MethodParam[] parameters )
			: base( name, parameters )
		{
		}

		#endregion Constructors

		#region Protected Members

		protected override string CreateStatementFormat()
		{
			return "CREATE PROCEDURE {0}\n{1}\nAS\n{2}\nRETURN";
		}

	    protected internal override string[] SqlObjectTypes
		{
            get
            {
                return new[] { "P", "PC" };    
            }
		}

		#endregion Protected Members

	}
}