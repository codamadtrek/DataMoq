using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace LazyE9.DataMock.Language
{
	public static class Evaluator
	{
		#region Evaluator Members

		/// <summary>
		/// Performs evaluation & replacement of independent sub-trees
		/// </summary>
		/// <param name="expression">The root of the expression tree.</param>
		/// <returns>A new tree with sub-trees evaluated and replaced.</returns>
		public static Expression ReduceSubtrees( Expression expression )
		{
			return ReduceSubtrees( expression, _CanBeEvaluatedLocally );
		}

		/// <summary>
		/// Performs evaluation & replacement of independent sub-trees
		/// </summary>
		/// <param name="expression">The root of the expression tree.</param>
		/// <param name="fnCanBeEvaluated">A function that decides whether a given expression node can be part of the local function.</param>
		/// <returns>A new tree with sub-trees evaluated and replaced.</returns>
		public static Expression ReduceSubtrees( Expression expression, Func<Expression, bool> fnCanBeEvaluated )
		{
			HashSet<Expression> nominations = new Nominator(fnCanBeEvaluated).Nominate(expression);
			return new SubtreeEvaluator( nominations ).Eval( expression );
		}

		#endregion Evaluator Members

		#region Private Members

		private static bool _CanBeEvaluatedLocally( Expression expression )
		{
			return 
				expression.NodeType == ExpressionType.Call
				? ((MethodCallExpression) expression).Method.DeclaringType != typeof (Param)
				: expression.NodeType != ExpressionType.Parameter;
		}

		#endregion Private Members

		#region Other

		/// <summary>
		/// Performs bottom-up analysis to determine which nodes can possibly
		/// be part of an evaluated sub-tree.
		/// </summary>
		private class Nominator : ExpressionVisitor
		{
			#region Constructors

			internal Nominator( Func<Expression, bool> fnCanBeEvaluated )
			{
				mFnCanBeEvaluated = fnCanBeEvaluated;
			}

			#endregion Constructors

			#region Nominator Members

			public override Expression Visit( Expression expression )
			{
				if( expression != null )
				{
					bool saveCannotBeEvaluated = mCannotBeEvaluated;
					mCannotBeEvaluated = false;
					base.Visit( expression );
					if( !mCannotBeEvaluated )
					{
						if( mFnCanBeEvaluated( expression ) )
						{
							mCandidates.Add( expression );
						}
						else
						{
							mCannotBeEvaluated = true;
						}
					}
					mCannotBeEvaluated |= saveCannotBeEvaluated;
				}
				return expression;
			}

			#endregion Nominator Members

			#region Fields

			private readonly Func<Expression, bool> mFnCanBeEvaluated;
			private bool mCannotBeEvaluated;
			private HashSet<Expression> mCandidates;

			#endregion Fields

			#region Internal Members

			internal HashSet<Expression> Nominate( Expression expression )
			{
				mCandidates = new HashSet<Expression>();
				Visit( expression );
				return mCandidates;
			}

			#endregion Internal Members

		}

		/// <summary>
		/// Evaluates & replaces sub-trees when first candidate is reached (top-down)
		/// </summary>
		private class SubtreeEvaluator : ExpressionVisitor
		{
			#region Constructors

			internal SubtreeEvaluator( HashSet<Expression> candidates )
			{
				mCandidates = candidates;
			}

			#endregion Constructors

			#region SubtreeEvaluator Members

			public override Expression Visit( Expression exp )
			{
				if( exp == null )
				{
					return null;
				}
				if( mCandidates.Contains( exp ) )
				{
					return _Evaluate( exp );
				}
				return base.Visit( exp );
			}

			#endregion SubtreeEvaluator Members

			#region Fields

			private readonly HashSet<Expression> mCandidates;

			#endregion Fields

			#region Private Members

			private Expression _Evaluate( Expression e )
			{
				if( e.NodeType == ExpressionType.Constant )
				{
					return e;
				}
				LambdaExpression lambda = Expression.Lambda( e );
				Delegate fn = lambda.Compile();
				return Expression.Constant( fn.DynamicInvoke( null ), e.Type );
			}

			#endregion Private Members

			#region Internal Members

			internal Expression Eval( Expression exp )
			{
				return Visit( exp );
			}

			#endregion Internal Members

		}

		#endregion Other

	}
}
