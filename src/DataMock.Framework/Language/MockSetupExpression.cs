using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using LazyE9.DataMock.Properties;
using LazyE9.DataMock.Setup;

namespace LazyE9.DataMock.Language
{
	public class MockSetupExpression<TDataContext, TResult> : IDataMockSetup<TResult>
	{
		#region Constructors

		public MockSetupExpression( DataMock<TDataContext> dataMock, Expression<Func<TDataContext, IEnumerable<TResult>>> expression )
		{
			mDataMock = dataMock;
			Expression body = expression.Body;
			Func<DatabaseObject> dataObjectBuilder = null;
			switch( body.NodeType )
			{
				case ExpressionType.Call:
					_Initialize( (MethodCallExpression)body, out mDataObjectName, out dataObjectBuilder, out mResultBuilder );
					break;
				case ExpressionType.MemberAccess:
					_Initialize( (MemberExpression)body, out mDataObjectName, out dataObjectBuilder, out mResultBuilder );
					break;
			}

			if( mDataObjectName == null || dataObjectBuilder == null || mResultBuilder == null )
			{
				string message = string.Format( CultureInfo.CurrentCulture, Resources.SetupNotSupported, expression );
				throw new ArgumentException( message );
			}

			mDataObject = mDataMock.GetOrCreateDatabaseObject( mDataObjectName, dataObjectBuilder );
		}

		#endregion Constructors

		#region MockSetupExpression Members

		public void Returns( params TResult[] resultValues )
		{
			foreach( TResult resultValue in resultValues )
			{
				Result resultBuilder = mResultBuilder( resultValue );
				mDataObject.Add( resultBuilder );
			}
		}

		#endregion MockSetupExpression Members

		#region Fields

		private readonly DatabaseObject mDataObject;
		private readonly DataMock<TDataContext> mDataMock;
		private readonly Func<object, Result> mResultBuilder;
		private readonly string mDataObjectName;

		#endregion Fields

		#region Private Members

		private static void _Initialize( MethodCallExpression methodCall, out string dataObjectName, out Func<DatabaseObject> databaseObjectBuilder, out Func<object, Result> resultBuilder )
		{
			dataObjectName = null;
			databaseObjectBuilder = null;
			resultBuilder = null;

			MethodInfo method = methodCall.Method;
			var functionAttribute = method.GetCustomAttribute<FunctionAttribute>();
			if( functionAttribute != null )
			{
				MethodParam[] parameters = _ParseParameters( method );
				var isFunction = functionAttribute.IsComposable;

				var name = functionAttribute.Name;
				dataObjectName = name;

				databaseObjectBuilder = () =>
						isFunction
						? new Function( name, parameters ) as DatabaseObject
						: new Procedure( name, parameters );

				resultBuilder =
					data => new ConditionalResult( data, parameters, methodCall.Arguments.ToArray() );
			}
		}

		private static void _Initialize( MemberExpression memberExpression, out string dataObjectName, out Func<DatabaseObject> databaseObjectBuilder, out Func<object, Result> resultBuilder )
		{
			dataObjectName = null;
			databaseObjectBuilder = null;
			resultBuilder = null;

			if( memberExpression.Member.MemberType == MemberTypes.Property )
			{
				var propertyInfo = (PropertyInfo)memberExpression.Member;
				Type propertyType = propertyInfo.PropertyType; //IEnumerable<TTable>
				Type modelType = propertyType.GetGenericArguments().Single();
				var tableAttribute = modelType.GetCustomAttribute<TableAttribute>();

				if( tableAttribute != null )
				{
					string name = tableAttribute.Name;

					dataObjectName = name;
					databaseObjectBuilder = () => new View( name );
					resultBuilder = data => new Result( data );
				}
			}
		}

		private static MethodParam[] _ParseParameters( MethodInfo methodInfo )
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

		#endregion Private Members

	}
}