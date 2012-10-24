using System;

using DataMock.DataTests.DataObjects;

using LazyE9.DataMock;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataMock.DataTests
{
	[TestClass]
	public class MockSetupExpressionTests
	{
		#region MockSetupExpressionTests Members

		[TestMethod]
		public void MockInvalidCall()
		{
			_InvalidMockTest(
				"Expression cannot be mocked: context => context.Translate(null)",
				dataMock => dataMock
					.Setup( context => context.Translate<People>( null ) )
					.Returns() );
		}

		[TestMethod]
		public void MockInvalidProperty()
		{
			_InvalidMockTest( "Expression cannot be mocked: context => context.NonMockableProperty",
				dataMock => dataMock
					.Setup( context => context.NonMockableProperty )
					.Returns() );
		}

		[TestMethod]
		public void MockInvalidRandomExpression()
		{
			_InvalidMockTest(
				"Expression cannot be mocked: context => new [] {}",
				dataMock => dataMock
					.Setup( context => new object[] { } )
					.Returns( new object[] { } ) );
		}

		#endregion MockSetupExpressionTests Members

		#region Private Members

		private void _InvalidMockTest( string expectedExceptionMessage, Action<DataMock<DataMockDataContext>> mockSetup )
		{
			var dataMock = new DataMock<DataMockDataContext>();
			try
			{
				mockSetup( dataMock );
				Assert.Fail( "Should not be mocked" );
			}
			catch( Exception exception )
			{
				Console.WriteLine( exception.Message );
				Assert.AreEqual( exception.Message, expectedExceptionMessage );
			}
		}

		#endregion Private Members

	}
}
