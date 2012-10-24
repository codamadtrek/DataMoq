using System.Collections.Generic;

namespace DataMock.DataTests.DataObjects
{
	partial class DataMockDataContext
	{
		public IEnumerable<string> NonMockableProperty
		{
			get
			{
				return new string[0];
			}
		}
	}
}
