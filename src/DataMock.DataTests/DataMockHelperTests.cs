using System;

using DataMock.DataTests.DataObjects;

using LazyE9.DataMock;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataMock.DataTests
{
	[TestClass]
	public class DataMockHelperTests : TestBase
	{
		#region DataMockHelperTests Members

		[TestMethod]
		public void DropConstraintBasic()
		{
			try
			{
				_RunInsert();
				Assert.Fail( "No exception thrown when inserting a Parent without a Name" );
			}
			catch( Exception exception )
			{
				Console.WriteLine( exception );
			}

			using(var context = new DataMockDataContext())
			{
				context.DisableAllCheckConstraints<DataMockDataContext, Child>();	
			}

			_RunInsert("Ralph");
		}

		#endregion DataMockHelperTests Members

		#region Private Members

		private void _RunInsert(string name = null)
		{
			using( var context = new DataMockDataContext() )
			{
				context.Parents.InsertOnSubmit( new Parent
				{
					Name = name ?? string.Empty
				} );
				context.SubmitChanges();
			}
		}

		#endregion Private Members

	}
}
