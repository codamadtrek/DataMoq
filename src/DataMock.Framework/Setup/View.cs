using System;

namespace LazyE9.DataMock.Setup
{
	public class View : DatabaseObject
	{
		#region Constructors

		public View( string name )
			: base( name )
		{

		}

		#endregion Constructors

		#region Protected Members

		protected override string[] GetSqlObjectTypes()
		{
			return new[] { "V" };
		}

		#endregion Protected Members

		#region Internal Members

		protected internal override string CreateCreateDataObjectStatement()
		{
			string createDataObjectStatement = String.Format( "CREATE VIEW {0} AS {1}", DataObjectName, CreateResultsSelectStatement() );
			return createDataObjectStatement;
		}

		#endregion Internal Members

	}
}