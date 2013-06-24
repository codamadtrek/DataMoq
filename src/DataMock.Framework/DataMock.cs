using System;
using System.Collections.Generic;
using System.Data.Linq;
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

        public void Execute(string connectionString)
        {
            _LogSql(connectionString, () =>
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    var command = connection.CreateCommand();

                    foreach (DatabaseObject dataObject in mSetups.Values)
                    {
                        dataObject.Configure(connection);

                        string dropIfExistsStatement = _DropIfExistsStatement(dataObject);
                        string createStatement = dataObject.CreateCreateDataObjectStatement();

                        _Execute(command, dropIfExistsStatement);
                        _Execute(command, createStatement);
                    }
                }
            });
        }

        /// <summary>
        /// Removes the mocks from the database.
        /// </summary>
        public void RemoveMocks(string connectionString)
        {
            _LogSql(connectionString, () =>
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    var command = connection.CreateCommand();

                    foreach (DatabaseObject dataObject in mSetups.Values
                        .GroupBy(dataObject => dataObject.DataObjectName)
                        .Select(grouping => grouping.First()))
                    {
                        string dropIfExistsStatement = _DropMockStatement(dataObject);

                        _Execute(command, dropIfExistsStatement);
                    }
                }
            });
        }

        /// <summary>
        /// Setup mocking for a scalar UDF.
        /// </summary>
        public ISingleDataMockSetup<TResult> Setup<TResult>(Expression<Func<TDataContext, TResult>> expression)
        {
            return new MockSetupExpression<TDataContext, TResult>(this, expression);
        }

        /// <summary>
        /// Setup mocking for a table valued function or sproc that returns only one row.
        /// </summary>
        public ISingleDataMockSetup<TResult> Setup<TResult>(Expression<Func<TDataContext, ISingleResult<TResult>>> expression)
        {
            return new MockSetupExpression<TDataContext, TResult>(this, expression);
        }

        /// <summary>
        /// Setup mocking for a table valued function or sproc.
        /// </summary>
        public IMultipleDataMockSetup<TResult> Setup<TResult>(Expression<Func<TDataContext, IQueryable<TResult>>> expression)
        {
            return new MockSetupExpression<TDataContext, TResult>(this, expression);
        }

        /// <summary>
        /// Setup mocking for a table valued function or sproc.
        /// </summary>
        public IMultipleDataMockSetup<TTableRow> Setup<TTableRow>(Expression<Func<TDataContext, Table<TTableRow>>> source) where TTableRow : class
        {
            ParameterExpression contextParameter = source.Parameters[0];

            UnaryExpression invocation =
                Expression.TypeAs(
                    Expression.Invoke(source, contextParameter),
                    typeof(IQueryable<TTableRow>));

            Expression<Func<TDataContext, IQueryable<TTableRow>>> iQueryableExpression =
                Expression.Lambda<Func<TDataContext, IQueryable<TTableRow>>>(invocation, contextParameter);

            return new MockSetupExpression<TDataContext, TTableRow>(this, iQueryableExpression);
        }

        #endregion DataMock Members

        #region Fields

        private const string EXISTS_SINGLE_TYPE = "IF EXISTS(SELECT NULL FROM SYS.OBJECTS WHERE OBJECT_ID = OBJECT_ID('{0}') AND TYPE = '{1}')";
        private const string EXISTS_MULTIPLE_TYPES = "IF EXISTS(SELECT NULL FROM SYS.OBJECTS WHERE OBJECT_ID = OBJECT_ID('{0}') AND TYPE IN ({1}))";
        private readonly IDictionary<string, DatabaseObject> mSetups = new Dictionary<string, DatabaseObject>();

        #endregion Fields

        #region Private Members

        private string _DropIfExistsStatement(DatabaseObject databaseObject)
        {
            return _DropIfExitsStatement(databaseObject.SqlObjectTypes, databaseObject.DataObjectTypeName, databaseObject.DataObjectName);
        }

        private string _DropIfExitsStatement(string[] objectTypes, string objectTypeName, string objectName)
        {
            string[] types = objectTypes;
            string ifExistsStatement;
            if (types.Length == 1)
            {
                ifExistsStatement = String.Format(EXISTS_SINGLE_TYPE, objectName, types.Single());
            }
            else
            {
                string[] stringifiedTypes = types.Select(type => String.Format("'{0}'", type)).ToArray();
                string commaSeperatedTypes = String.Join(", ", stringifiedTypes);
                ifExistsStatement = String.Format(EXISTS_MULTIPLE_TYPES, objectName, commaSeperatedTypes);
            }
            string result = String.Format("{0}{1}DROP {2} {3}", ifExistsStatement, Environment.NewLine, objectTypeName, objectName);
            return result;
        }

        private string _DropMockStatement(DatabaseObject databaseObject)
        {
            return _DropIfExitsStatement(databaseObject.SqlObjectTypes, databaseObject.DataObjectTypeName, databaseObject.DataObjectName);
        }

        private void _Execute(SqlCommand command, string sqlCommand)
        {
            _Log(sqlCommand);
            command.CommandText = sqlCommand;
            command.ExecuteNonQuery();
        }

        private void _Log(string message)
        {
            if (Log != null)
            {
                Log.WriteLine(message);
            }
        }

        private void _LogSql(string connectionString, Action sqlOperations)
        {
            _Log(String.Format("*** DataMock ({0}) ***", connectionString));
            sqlOperations();
            _Log("*** DataMock ***");
        }

        #endregion Private Members

        #region Internal Members

        internal DatabaseObject GetOrCreateDatabaseObject(string databaseObjectName, Func<DatabaseObject> creator)
        {
            DatabaseObject result;
            if (!mSetups.TryGetValue(databaseObjectName, out result))
            {
                result = creator();
                mSetups.Add(databaseObjectName, result);
            }
            return result;
        }

        #endregion Internal Members

    }
}