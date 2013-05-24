using System;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace LazyE9.DataMock
{
	public static class Param
	{
		#region Param Members

//		public static TValue Is<TValue>( Expression<Func<TValue, bool>> match )
//		{
//			return Match.Create(
//				value => match.Compile().Invoke( value ),
//				() => Is( match ) );
//		}

		public static TValue IsAny<TValue>()
		{
			return Match.Create( value => true, () => IsAny<TValue>() );
		}

//		public static TValue IsInRange<TValue>( TValue from, TValue to, Range rangeKind )
//			where TValue : IComparable
//		{
//			return Match.Create( value =>
//			{
//				if( value == null )
//				{
//					return false;
//				}
//
//				if( rangeKind == Range.Exclusive )
//				{
//					return value.CompareTo( from ) > 0 && value.CompareTo( to ) < 0;
//				}
//
//				return value.CompareTo( from ) >= 0 && value.CompareTo( to ) <= 0;
//			},
//			() => IsInRange( from, to, rangeKind ) );
//		}
//
//		public static string IsRegex( string regex )
//		{
//			// The regex is constructed only once.
//			var re = new Regex( regex );
//
//			// But evaluated every time :)
//			return Match.Create( re.IsMatch, () => IsRegex( regex ) );
//		}
//
//		public static string IsRegex( string regex, RegexOptions options )
//		{
//			// The regex is constructed only once.
//			var re = new Regex( regex, options );
//
//			// But evaluated every time :)
//			return Match.Create( re.IsMatch, () => IsRegex( regex, options ) );
//		}

		#endregion Param Members

	}
}
