using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq.Expressions;

using LazyE9.DataMock.Properties;

namespace LazyE9.DataMock
{
	[DebuggerStepThrough]
	internal static class Guard
	{
		#region Guard Members

		public static void CanBeAssigned( Expression<Func<object>> reference, Type typeToAssign, Type targetType )
		{
			if( !targetType.IsAssignableFrom( typeToAssign ) )
			{
				if( targetType.IsInterface )
				{
					throw new ArgumentException( string.Format(
						CultureInfo.CurrentCulture,
						Resources.TypeNotImplementInterface,
						typeToAssign,
						targetType ), _GetParameterName( reference ) );
				}

				throw new ArgumentException( string.Format(
					CultureInfo.CurrentCulture,
					Resources.TypeNotInheritFromType,
					typeToAssign,
					targetType ), _GetParameterName( reference ) );
			}
		}

		/// <summary>
		/// Ensures the given <paramref name="value"/> is not null.
		/// Throws <see cref="ArgumentNullException"/> otherwise.
		/// </summary>
		public static void NotNull<T>( Expression<Func<T>> reference, T value )
		{
			if( value == null )
			{
				throw new ArgumentNullException( _GetParameterName( reference ) );
			}
		}

		/// <summary>
		/// Ensures the given string <paramref name="value"/> is not null or empty.
		/// Throws <see cref="ArgumentNullException"/> in the first case, or 
		/// <see cref="ArgumentException"/> in the latter.
		/// </summary>
		public static void NotNullOrEmpty( Expression<Func<string>> reference, string value )
		{
			NotNull<string>( reference, value );
			if( value.Length == 0 )
			{
				throw new ArgumentException( Resources.ArgumentCannotBeEmpty, _GetParameterName( reference ) );
			}
		}

		/// <summary>
		/// Checks an argument to ensure it is in the specified range excluding the edges.
		/// </summary>
		/// <typeparam name="T">Type of the argument to check, it must be an <see cref="IComparable"/> type.
		/// </typeparam>
		/// <param name="reference">The expression containing the name of the argument.</param>
		/// <param name="value">The argument value to check.</param>
		/// <param name="from">The minimun allowed value for the argument.</param>
		/// <param name="to">The maximun allowed value for the argument.</param>
		public static void NotOutOfRangeExclusive<T>( Expression<Func<T>> reference, T value, T from, T to )
			where T : IComparable
		{
			if( value != null && (value.CompareTo( from ) <= 0 || value.CompareTo( to ) >= 0) )
			{
				throw new ArgumentOutOfRangeException( _GetParameterName( reference ) );
			}
		}

		/// <summary>
		/// Checks an argument to ensure it is in the specified range including the edges.
		/// </summary>
		/// <typeparam name="T">Type of the argument to check, it must be an <see cref="IComparable"/> type.
		/// </typeparam>
		/// <param name="reference">The expression containing the name of the argument.</param>
		/// <param name="value">The argument value to check.</param>
		/// <param name="from">The minimun allowed value for the argument.</param>
		/// <param name="to">The maximun allowed value for the argument.</param>
		public static void NotOutOfRangeInclusive<T>( Expression<Func<T>> reference, T value, T from, T to )
			where T : IComparable
		{
			if( value != null && (value.CompareTo( from ) < 0 || value.CompareTo( to ) > 0) )
			{
				throw new ArgumentOutOfRangeException( _GetParameterName( reference ) );
			}
		}

		public static void SameSize<TArray1, TArray2>( TArray1[] array1, TArray2[] array2 )
		{
			if( array1.Length != array2.Length )
			{
				throw new ArgumentException( "Arguments should be of the same lenght." );
			}
		}

		#endregion Guard Members

		#region Private Members

		private static string _GetParameterName( LambdaExpression reference )
		{
			var member = (MemberExpression)reference.Body;
			return member.Member.Name;
		}

		#endregion Private Members

	}
}