using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

namespace LazyE9.DataMock
{
	public class Match<T> : Match
	{
		#region Constructors

		internal Match( Predicate<T> condition )
			: this( condition, DefaultRender )
		{
		}

		internal Match( Predicate<T> condition, Expression<Func<T>> renderExpression )
		{
			Condition = condition;
			RenderExpression = renderExpression.Body;
		}

		#endregion Constructors

		#region Fields

		private static readonly Expression<Func<T>> DefaultRender = Expression.Lambda<Func<T>>(
			Expression.Call(
				typeof( Match )
					.GetMethod( "Matcher", BindingFlags.Static | BindingFlags.NonPublic )
					.MakeGenericMethod( typeof( T ) ) ) );

		#endregion Fields

		#region Internal Members

		internal Predicate<T> Condition
		{
			get;
			set;
		}

		internal override bool Matches( object value )
		{
			if( value != null && !(value is T) )
			{
				return false;
			}

			return Condition( (T)value );
		}

		#endregion Internal Members

	}

	public abstract class Match
	{
		#region Match Members

		public static T Create<T>( Predicate<T> condition )
		{
			_SetLastMatch( new Match<T>( condition ) );
			return default( T );
		}

		[SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures" )]
		public static T Create<T>( Predicate<T> condition, Expression<Func<T>> renderExpression )
		{
			_SetLastMatch( new Match<T>( condition, renderExpression ) );
			return default( T );
		}

		#endregion Match Members

		#region Private Members

		/// <devdoc>
		/// This method is used to set an expression as the last matcher invoked, 
		/// which is used in the SetupSet to allow matchers in the prop = value 
		/// delegate expression. This delegate is executed in "fluent" mode in 
		/// order to capture the value being set, and construct the corresponding 
		/// methodcall.
		/// This is also used in the MatcherFactory for each argument expression.
		/// This method ensures that when we execute the delegate, we 
		/// also track the matcher that was invoked, so that when we create the 
		/// methodcall we build the expression using it, rather than the null/default 
		/// value returned from the actual invocation.
		/// </devdoc>
		private static Match<TValue> _SetLastMatch<TValue>( Match<TValue> match )
		{
			//if( FluentMockContext.IsActive )
			//{
			//	FluentMockContext.Current.LastMatch = match;
			//}

			return match;
		}

		#endregion Private Members

		#region Internal Members

		internal Expression RenderExpression
		{
			get;
			set;
		}

		/// <devdoc>
		/// Provided for the sole purpose of rendering the delegate passed to the 
		/// matcher constructor if no friendly render lambda is provided.
		/// </devdoc>
		internal static TValue Matcher<TValue>()
		{
			return default( TValue );
		}

		internal abstract bool Matches( object value );

		#endregion Internal Members

	}
}