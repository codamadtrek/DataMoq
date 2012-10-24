using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Linq.Expressions;

using LazyE9.DataMock.Language;
using LazyE9.DataMock.Setup;

namespace LazyE9.DataMock
{
	public class DataMock<TDataContext>
	{
		#region DataMock Members

		public TextWriter Log
		{
			get;
			set;
		}

		public void Execute( string connectionString )
		{
			_Log( String.Format( "*** DataMock ({0}) ***", connectionString ) );

			using( var connection = new SqlConnection( connectionString ) )
			{
				connection.Open();
				var command = connection.CreateCommand();

				foreach( DatabaseObject dataObject in mSetups.Values )
				{
					string dropIfExistsStatement = _DropIfExistsStatement( dataObject );
					string createStatement = dataObject.CreateCreateDataObjectStatement();

					_Execute( command, dropIfExistsStatement );
					_Execute( command, createStatement );
				}
			}

			_Log( "*** DataMock ***");
		}

		public IDataMockSetup<TResult> Setup<TResult>( Expression<Func<TDataContext, IEnumerable<TResult>>> expression )
		{
			return new MockSetupExpression<TDataContext, TResult>( this, expression );
		}

		#endregion DataMock Members

		#region Fields

		private const string EXISTS_SINGLE_TYPE = "IF EXISTS(SELECT NULL FROM SYS.OBJECTS WHERE OBJECT_ID = OBJECT_ID('{0}') AND TYPE = '{1}')";
		private const string EXISTS_MULTIPLE_TYPES = "IF EXISTS(SELECT NULL FROM SYS.OBJECTS WHERE OBJECT_ID = OBJECT_ID('{0}') AND TYPE IN ({1}))";
		private readonly IDictionary<string, DatabaseObject> mSetups = new Dictionary<string, DatabaseObject>();

		#endregion Fields

		#region Private Members

		private string _DropIfExistsStatement( DatabaseObject databaseObject )
		{
			string[] types = databaseObject.SqlObjectTypes.ToArray();
			string ifExistsStatement;
			if( types.Length == 1 )
			{
				ifExistsStatement = String.Format( EXISTS_SINGLE_TYPE, databaseObject.DataObjectName, types.Single() );
			}
			else
			{
				string[] stringifiedTypes = types.Select( type => String.Format( "'{0}'", type ) ).ToArray();
				string commaSeperatedTypes = String.Join( ", ", stringifiedTypes );
				ifExistsStatement = String.Format( EXISTS_MULTIPLE_TYPES, databaseObject.DataObjectName, commaSeperatedTypes );
			}
			string result = String.Format( "{0}{1}{2}", ifExistsStatement, Environment.NewLine, databaseObject.CreateDropStatement() );
			return result;
		}

		private void _Execute( SqlCommand command, string sqlCommand )
		{
			_Log( sqlCommand );
			command.CommandText = sqlCommand;
			command.ExecuteNonQuery();
		}

		private void _Log( string message )
		{
			if( Log != null )
			{
				Log.WriteLine( message );
			}
		}

		#endregion Private Members

		#region Internal Members

		internal DatabaseObject GetOrCreateDatabaseObject( string databaseObjectName, Func<DatabaseObject> creator )
		{
			DatabaseObject result;
			if( !mSetups.TryGetValue( databaseObjectName, out result ) )
			{
				result = creator();
				mSetups.Add( databaseObjectName, result );
			}
			return result;
		}

		#endregion Internal Members

	}
}