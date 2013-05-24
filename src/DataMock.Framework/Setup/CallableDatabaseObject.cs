using System.Linq;

namespace LazyE9.DataMock.Setup
{
	internal abstract class CallableDatabaseObject : DatabaseObject
	{
		#region Constructors

		protected CallableDatabaseObject( string name, MethodParam[] parameters )
			: base( name )
		{
			Parameters = parameters;
		}

	    #endregion Constructors

		#region Protected Members

		protected MethodParam[] Parameters
		{
			get;
			private set;
		}

		protected abstract string CreateStatementFormat();

		#endregion Protected Members

		#region Internal Members

		protected internal override string CreateCreateDataObjectStatement()
		{
			string[] functionParameterDeclarations =
				Parameters
					.Select( param => string.Concat( "@", param.ParameterName, " ", param.ParameterType ) )
					.ToArray();
			string parametersDeclaration = string.Join( "\n,", functionParameterDeclarations );


			string format = CreateStatementFormat();
			string statement = string.Format( format, DataObjectName, parametersDeclaration, CreateResultsSelectStatement() );
			return statement;
		}

	    #endregion Internal Members

	}
}