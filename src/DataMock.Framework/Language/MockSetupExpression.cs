using System;
using System.Collections;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using LazyE9.DataMock.Properties;
using LazyE9.DataMock.Setup;

namespace LazyE9.DataMock.Language
{
    public class MockSetupExpression<TDataContext, TResult> : IMultipleDataMockSetup<TResult>, ISingleDataMockSetup<TResult>
    {
        #region Constructors

        public MockSetupExpression(DataMock<TDataContext> dataMock, Expression<Func<TDataContext, TResult>> expression)
        {
            _CreateMockSetupExpression(dataMock, expression);
        }

        public MockSetupExpression(DataMock<TDataContext> dataMock, Expression<Func<TDataContext, IQueryable<TResult>>> expression)
        {
            _CreateMockSetupExpression(dataMock, expression);
        }

        public MockSetupExpression(DataMock<TDataContext> dataMock, Expression<Func<TDataContext, ISingleResult<TResult>>> expression)
        {
            _CreateMockSetupExpression(dataMock, expression);
        }

        #endregion Constructors

        #region MockSetupExpression Members

        public void Returns()
        {

        }

        public void Returns(params TResult[] resultValues)
        {
            foreach (TResult resultValue in resultValues)
            {
                Returns(resultValue);
            }
        }

        public void Returns(TResult result)
        {
            Result resultBuilder = mResultBuilder(result);
            mDataObject.Add(resultBuilder);
        }

        #endregion MockSetupExpression Members

        #region Fields

        private DatabaseObject mDataObject;
        private DataMock<TDataContext> mDataMock;
        private Func<object, Result> mResultBuilder;
        private string mDataObjectName;

        #endregion Fields

        #region Private Members

        private void _CreateMockSetupExpression(DataMock<TDataContext> dataMock, LambdaExpression expression)
        {
            mDataMock = dataMock;
            Expression body = expression.Body;
            Func<DatabaseObject> dataObjectBuilder;

            _VisitExpression(body, out dataObjectBuilder);

            if (mDataObjectName == null || dataObjectBuilder == null || mResultBuilder == null)
            {
                string message = string.Format(CultureInfo.CurrentCulture, Resources.SetupNotSupported, expression);
                throw new ArgumentException(message);
            }

            mDataObject = mDataMock.GetOrCreateDatabaseObject(mDataObjectName, dataObjectBuilder);
        }

        private static void _Initialize(MethodCallExpression methodCall, out string dataObjectName, out Func<DatabaseObject> databaseObjectBuilder, out Func<object, Result> resultBuilder)
        {
            dataObjectName = null;
            databaseObjectBuilder = null;
            resultBuilder = null;

            MethodInfo method = methodCall.Method;
            var functionAttribute = method.GetCustomAttribute<FunctionAttribute>();
            if (functionAttribute != null)
            {
                MethodParam[] parameters = _ParseParameters(method);
                bool isFunction = functionAttribute.IsComposable;
                bool isTableValuedFunction = isFunction && typeof (IQueryable).IsAssignableFrom(method.ReturnType);

                var name = functionAttribute.Name;
                dataObjectName = name;

                databaseObjectBuilder = () =>
                        isFunction
                        ? (isTableValuedFunction ? new TableValuedFunction(name, parameters) : new ScalarFunction(name, parameters, method.ReturnType) as DatabaseObject)
                        : new Procedure(name, parameters);

                resultBuilder = data => new ConditionalResult(data, parameters, methodCall.Arguments.ToArray());
            }
        }

        private static void _Initialize(MemberExpression memberExpression, out string dataObjectName, out Func<DatabaseObject> databaseObjectBuilder, out Func<object, Result> resultBuilder)
        {
            dataObjectName = null;
            databaseObjectBuilder = null;
            resultBuilder = null;

            if (memberExpression.Member.MemberType == MemberTypes.Property)
            {
                var propertyInfo = (PropertyInfo)memberExpression.Member;
                Type propertyType = propertyInfo.PropertyType; //IEnumerable<TTable>
                Type modelType = propertyType.GetGenericArguments().Single();
                var tableAttribute = modelType.GetCustomAttribute<TableAttribute>();

                if (tableAttribute != null)
                {
                    string name = tableAttribute.Name;

                    dataObjectName = name;
                    databaseObjectBuilder = () => new View(name);
                    resultBuilder = data => new Result(data);
                }
            }
        }

        private static MethodParam[] _ParseParameters(MethodInfo methodInfo)
        {
            ParameterInfo[] paramInfo = methodInfo.GetParameters();
            var paramAttributes =
                from param in paramInfo
                select new
                {
                    Attribute = param.GetCustomAttribute<ParameterAttribute>(),
                    Info = param
                };

            MethodParam[] methodParams =
                (from param in paramAttributes
                 select new MethodParam
                 {
                     ParameterName = param.Attribute.Name ?? param.Info.Name,
                     ParameterType = param.Attribute.DbType
                 }).ToArray();

            return methodParams;
        }

        private void _VisitExpression(Expression expression, out Func<DatabaseObject> dataObjectBuilder)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.Call:
                    _Initialize((MethodCallExpression)expression, out mDataObjectName, out dataObjectBuilder, out mResultBuilder);
                    break;
                case ExpressionType.MemberAccess:
                    _Initialize((MemberExpression)expression, out mDataObjectName, out dataObjectBuilder, out mResultBuilder);
                    break;
                case ExpressionType.TypeAs:
                    _VisitExpression(((UnaryExpression)expression).Operand, out dataObjectBuilder);
                    break;
                case ExpressionType.Invoke:
                    _VisitExpression(((InvocationExpression)expression).Expression, out dataObjectBuilder);
                    break;
                case ExpressionType.Lambda:
                    _VisitExpression(((LambdaExpression)expression).Body, out dataObjectBuilder);
                    break;
                default:
                    dataObjectBuilder = null;
                    break;
            }
        }

        #endregion Private Members

    }
}