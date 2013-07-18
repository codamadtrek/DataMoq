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

        #endregion DatabaseObject Members

        #region Fields

        private readonly IList<Result> mResults = new List<Result>();

        #endregion Fields

        #region Protected Members

        protected string CreateResultsSelectStatement()
        {
            string[] clauses = mResults.Select(result => result.GetSelectStatement()).ToArray();
            return string.Join("\nUNION ALL\n", clauses);
        }

        #endregion Protected Members

        #region Internal Members

        protected internal virtual string DataObjectTypeName
        {
            get
            {
                return GetType().Name.ToUpperInvariant();
            }
        }

        protected internal virtual string MockDataObjectTypeName
        {
            get { return DataObjectName; }
        }

        protected internal virtual string[] PostCreateStatements
        {
            get
            {
                return new string[0];
            }
        }

        protected internal abstract string[] SqlObjectTypes { get; }

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