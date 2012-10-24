using System.Linq;
using System.Reflection;

namespace LazyE9.DataMock
{
	public static class CustomAttributeProviderExtensions
	{
		#region CustomAttributeProviderExtensions Members

		public static TAttribute GetCustomAttribute<TAttribute>( this ICustomAttributeProvider info )
		{
			return
				info.GetCustomAttributes( typeof( TAttribute ), true )
					.Cast<TAttribute>()
					.SingleOrDefault();
		}

		#endregion CustomAttributeProviderExtensions Members

	}
}
