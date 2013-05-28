using System;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

using LazyE9.DataMock.Language;
using LazyE9.DataMock.Setup;

namespace LazyE9.DataMock
{
    public static class DataMockHelper
    {
        #region DataMockHelper Members

        public static void DefaultValues(object target,
            int defaultNumeric = 1,
            string defaultString = "A",
            char defaultChar = 'A',
            DateTime? defaultDateTime = null,
            bool defaultBool = false,
            bool preferNullValue = false)
        {
            var properties = target.GetType().GetProperties();

            foreach (var property in properties)
            {
                if (property.CanWrite)
                {
                    object propertyValue = null;
                    var propertyType = Nullable.GetUnderlyingType(property.PropertyType);
                    bool isNullable;
                    if (propertyType == null)
                    {
                        propertyType = property.PropertyType;
                        isNullable = !propertyType.IsValueType;
                    }
                    else
                    {
                        isNullable = true;
                    }

                    if (!isNullable || !preferNullValue)
                    {
                        if (propertyType == typeof(string))
                        {
                            propertyValue = defaultString;
                        }
                        else if (propertyType == typeof(int) ||
                            propertyType == typeof(short) ||
                            propertyType == typeof(byte) ||
                            propertyType == typeof(decimal) ||
                            propertyType == typeof(double) ||
                            propertyType == typeof(long) ||
                            propertyType == typeof(float))
                        {
                            propertyValue = Convert.ChangeType(defaultNumeric, propertyType);
                        }
                        else if (propertyType == typeof(bool))
                        {
                            propertyValue = defaultBool;
                        }
                        else if (propertyType == typeof(char))
                        {
                            propertyValue = defaultChar;
                        }
                        else if (propertyType == typeof(Guid))
                        {
                            propertyValue = Guid.NewGuid();
                        }
                        else if (propertyType == typeof(DateTime))
                        {
                            propertyValue = defaultDateTime.GetValueOrDefault(new DateTime(1753, 1, 1));
                        }
                        else if (propertyType.IsValueType)
                        {
                            throw new InvalidOperationException(string.Format(
                                "You will needd to add this property type to this method. {0}", property.PropertyType));
                        }
                    }

                    property.SetValue(target, propertyValue, null);
                }
            }
        }

        public static void DisableAllCheckConstraints(string tableName, string connectionString)
        {
            var disableStatement = string.Format("ALTER TABLE {0} NOCHECK CONSTRAINT ALL", tableName);

            using (var sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();
                var sqlCommand = new SqlCommand(disableStatement, sqlConnection);

                sqlCommand.ExecuteNonQuery();
            }
        }

        public static void DisableAllCheckConstraints<TDataContext, TTable>(this TDataContext dataContext)
            where TDataContext : DataContext
            where TTable : class
        {
            var tableMapping = dataContext.Mapping.GetTable(typeof(TTable));
            string tableName = tableMapping.TableName;
            dataContext.DisableAllCheckConstraints(tableName);
        }

        public static void DisableAllCheckConstraints<TDataContext>(this TDataContext dataContext, string tableName)
            where TDataContext : DataContext
        {
            string connectionString = dataContext.Connection.ConnectionString;
            DisableAllCheckConstraints(tableName, connectionString);
        }

        public static void DropAndCreateMockFunction(string functionName, string createMockFuctionStatement, string connectionString)
        {
            var dropStatement = string.Format(
                "IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'{0}') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))"
                    + "\n DROP FUNCTION {0}", functionName);

            _DropAndCreateDataObject(createMockFuctionStatement, connectionString, dropStatement);
        }

        public static void DropAndCreateMockProcedure(string procedureName, string createMockFuctionStatement, string connectionString)
        {
            var dropStatement = string.Format(
                "IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'{0}') AND type in (N'P', N'PC'))"
                + "\n DROP PROCEDURE {0}", procedureName);

            _DropAndCreateDataObject(createMockFuctionStatement, connectionString, dropStatement);
        }

        public static string FormatValue(object value)
        {
            string result;
            dynamic dynoValue = value;
            if (value == null)
            {
                result = "NULL";
            }
            else if (Nullable.GetUnderlyingType(value.GetType()) != null)
            {
                if (dynoValue.HasValue)
                {
                    value = dynoValue.Value;
                    result = _FormatNonNullable(value);
                }
                else
                {
                    result = "NULL";
                }
            }
            else
            {
                result = _FormatNonNullable(value);
            }

            return result;
        }

        public static string GetDatabaseType(Type propertyType, ColumnAttribute columnAttribute)
        {
            string databaseType;
            if (columnAttribute == null)
            {
                propertyType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
                if (propertyType == typeof(Guid))
                {
                    databaseType = "UNIQUEIDENTIFIER";
                }
                else if (propertyType == typeof(string))
                {
                    databaseType = "VARCHAR(900)";
                }
                else
                {
                    databaseType = propertyType.Name;
                }
            }
            else
            {
                databaseType = columnAttribute.DbType.Replace("NOT NULL", string.Empty).Trim();
            }

            return databaseType;
        }

        public static string ResultToSelectStatement(object result, string whereClause)
        {
            var targetType = result.GetType();
            PropertyInfo[] simpleProperties =
                targetType != typeof(string)
                    ? targetType
                        .GetProperties()
                        .Where(prop => prop.GetIndexParameters().Length == 0)
                        .ToArray()
                    : new PropertyInfo[0];


            string fields =
                simpleProperties.Length == 0
                ? string.Format("{0} AS RESULT", FormatValue(result))
                : string.Join(", ", simpleProperties.Select(
                prop =>
                {
                    var columnAttribute = prop
                        .GetCustomAttributes(typeof(ColumnAttribute), true)
                        .Cast<ColumnAttribute>()
                        .FirstOrDefault();

                    var databaseType = GetDatabaseType(prop.PropertyType, columnAttribute);

                    return string.Format("CAST( {0} AS {1}) AS {2}",
                        FormatValue(prop.GetValue(result, null)),
                        databaseType,
                        prop.Name);
                }).ToArray());

            return string.Format("(SELECT {0} {1})", fields, whereClause);
        }

        #endregion DataMockHelper Members

        #region Private Members

        private static void _DropAndCreateDataObject(string createMockFuctionStatement, string connectionString, string dropStatement)
        {
            var sqlConnection = new SqlConnection(connectionString);
            sqlConnection.Open();

            var dropCommand = new SqlCommand(dropStatement, sqlConnection);
            dropCommand.ExecuteNonQuery();

            var recreateCommand = new SqlCommand(createMockFuctionStatement, sqlConnection);
            recreateCommand.ExecuteNonQuery();
        }

        private static string _FormatNonNullable(object value)
        {
            string result;

            if (value == null)
            {
                result = "NULL";
            }
            else if (value is bool)
            {
                result = "0";
                if ((bool)value)
                {
                    result = "1";
                }
            }
            else if (value is string || value is Guid)
            {
                result = string.Format("'{0}'", value);
            }
            else if (value is DateTime)
            {
                if (((DateTime)value) < new DateTime(1753, 12, 31))
                {
                    value = new DateTime(1753, 12, 31);
                }

                result = string.Format("'{0}'", value);
            }
            else
            {
                result = value.ToString();
            }

            return result;
        }

        private static string _ToValueString(Expression expression)
        {
            return ExpressionStringBuilder.GetString(expression);
        }

        #endregion Private Members

        #region Internal Members

        internal static string ArgumentsToWhereClause(Expression[] arguments, MethodParam[] methodParams)
        {
            Guard.SameSize(arguments, methodParams);

            var whereClause = new StringBuilder();

            string conjunction = "WHERE ";
            for (int index = 0; index < arguments.Length; index++)
            {
                string value = _ToValueString(arguments[index]);

                if (!string.IsNullOrWhiteSpace(value))
                {
                    whereClause.AppendFormat("{0} @{1} = CAST({2} AS {3})",
                        conjunction, methodParams[index].ParameterName,
                        value, methodParams[index].ParameterType);

                    conjunction = " AND ";
                }
            }

            return whereClause.ToString();
        }

        #endregion Internal Members

    }
}
