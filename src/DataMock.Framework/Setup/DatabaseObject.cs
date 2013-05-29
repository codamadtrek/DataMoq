using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace LazyE9.DataMock.Setup
{
    public abstract class DatabaseObject
    {
        #region Constructors

        protected DatabaseObject(string name)
        {
            DataObjectName = name;
        }

        #endregion Constructors

        #region DatabaseObject Members

        public string DataObjectName
        {
            get;
            private set;
        }

        public string CreateDropStatement()
        {
            string dropStatement = String.Format("DROP {0} {1}", DataObjectTypeName, DataObjectName);
            return dropStatement;
        }

        #endregion DatabaseObject Members

        #region Fields

        private readonly IList<Result> mResults = new List<Result>();

        #endregion Fields

        #region Protected Members

        protected virtual string DataObjectTypeName
        {
            get
            {
                return GetType().Name.ToUpperInvariant();
            }
        }

        protected string CreateResultsSelectStatement()
        {
            string[] clauses = mResults.Select(result => result.GetSelectStatement()).ToArray();
            return string.Join("\nUNION ALL\n", clauses);
        }

        protected abstract string[] GetSqlObjectTypes();

        #endregion Protected Members

        #region Internal Members

        protected internal IEnumerable<string> SqlObjectTypes
        {
            get
            {
                return GetSqlObjectTypes();
            }
        }

        internal void Add(Result result)
        {
            mResults.Add(result);
        }

        /// <summary>
        /// Call back method invoked before CreateCreateDataObjectStatement(). Inheriting types can override this method to interrogate
        /// the database for any additional information they need to build the create statement.
        /// </summary>
        /// <param name="connection">The connection to the database</param>
        protected internal virtual void Configure(DbConnection connection)
        {
            
        }

        protected internal abstract string CreateCreateDataObjectStatement();

        #endregion Internal Members

    }
}