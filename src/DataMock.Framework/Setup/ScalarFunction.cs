using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;

namespace LazyE9.DataMock.Setup
{
    internal class ScalarFunction : Function
    {
        #region Constructors

        public ScalarFunction(string name, MethodParam[] parameters, Type returnType)
            : base(name, parameters)
        {
            mReturnType = Nullable.GetUnderlyingType(returnType) ?? returnType;
        }

        #endregion Constructors

        #region Fields

        private readonly Regex mSchemaReplacer = new Regex(@"\w+\.");
        private readonly Type mReturnType;
        private string mDataType = "SQL_VARIANT";

        #endregion Fields

        #region Protected Members

        protected override string CreateStatementFormat()
        {

            return "CREATE FUNCTION {0}\n(\n{1}\n)\nRETURNS " + mDataType + " AS \nBEGIN\nRETURN (SELECT TOP 1 RESULT FROM \n(\n{2}\n) AS CANDIDATES\n)\nEND";
        }

        #endregion Protected Members

        #region Internal Members

        protected internal override string[] SqlObjectTypes
        {
            get
            {
                return new[] { "FN", "IF", "TF", "FS", "FT" };
            }
        }

        /// <summary>
        /// Interrogates the database for the correct return type of the function.
        /// </summary>
        protected internal override void Configure(DbConnection connection)
        {
            using (DbCommand command = connection.CreateCommand())
            {
                command.CommandText = @"
                    SELECT [DATA_TYPE]
                          ,[CHARACTER_MAXIMUM_LENGTH]
                          ,[NUMERIC_PRECISION]
                          ,[NUMERIC_SCALE]
                      FROM [INFORMATION_SCHEMA].[ROUTINES]
                      WHERE [ROUTINE_NAME] = @RoutineName";

                string name = mSchemaReplacer.Replace(DataObjectName, string.Empty);
                command.Parameters.Add(new SqlParameter("@RoutineName", name));

                using (DbDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string dataType = reader.GetString(0);
                        int characterMaximumLength = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
                        int numericPrecision = reader.IsDBNull(2) ? 0 : reader.GetByte(2);
                        int numericScale = reader.IsDBNull(3) ? 0 : reader.GetInt32(3);

                        var numbers = new int[2];
                        numbers[0] = characterMaximumLength + numericPrecision; //one of these numbers will always be zero
                        numbers[1] = numericScale;

                        numbers = numbers.Where(number => number > 0).ToArray();

                        mDataType =
                            numbers.Length > 0 && mReturnType != typeof(int)
                            ? string.Format("{0}({1})", dataType, string.Join(", ", numbers))
                            : dataType;
                    }
                }
            }

        }

        #endregion Internal Members

    }
}