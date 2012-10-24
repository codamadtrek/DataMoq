using System.Linq.Expressions;

namespace LazyE9.DataMock.Setup
{
	internal class ConditionalResult : Result
	{
		#region Constructors

		public ConditionalResult( object result, MethodParam[] methodParams, Expression[] criteria )
			: base( result )
		{
			mCriteria = criteria;
			mMethodParams = methodParams;
		}

		#endregion Constructors

		#region ConditionalResult Members

		public override string GetSelectStatement()
		{
			string whereClause = DataMockHelper.ArgumentsToWhereClause( mCriteria, mMethodParams );
			return DataMockHelper.ResultToSelectStatement( ResultData, whereClause );
		}

		#endregion ConditionalResult Members

		#region Fields

		private readonly Expression[] mCriteria;
		private readonly MethodParam[] mMethodParams;

		#endregion Fields

	}
}