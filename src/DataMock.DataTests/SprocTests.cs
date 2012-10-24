using System;
using System.Linq;

using DataMock.DataTests.DataObjects;
using DataMock.DataTests.Properties;

using LazyE9.DataMock;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataMock.DataTests
{
	[TestClass]
	public class SprocTests : TestBase
	{
		#region SprocTests Members

		[TestMethod]
		public void NoParameterProcedureReplacement()
		{
			var dataMock = new DataMock<DataMockDataContext>
			{
				Log = Console.Out
			};

			const string TEXT_MESSAGE = "This is a test and nothing but a test";

			dataMock.Setup( dc => dc.NoParametersProcB() )
				.Returns( new NoParametersProcBResult
				{
					ID = 5,
					TextMessage = TEXT_MESSAGE
				} );

			dataMock.Execute( Settings.Default.DataMockConnectionString );

			var dataContext = new DataMockDataContext();

			var result = dataContext.NoParametersProcA().ToArray();

			Assert.AreEqual( result[0].TextMessage, TEXT_MESSAGE );
			Assert.AreEqual( result[0].Id, 5 );
			Assert.AreEqual( result[1].TextMessage, "Message from Proc A" );
			Assert.AreEqual( result[1].Id, 10 );

		}

		[TestMethod]
		public void VariableOutputProcedureReplacementWithAnyParam()
		{
			var dataMock = new DataMock<DataMockDataContext>
			{
				Log = Console.Out
			};

			const string TEXT_MESSAGE = "This is a test and nothing but a test";

			dataMock.Setup( dc => dc.VariableOutputProcB( Param.IsAny<int>() ) )
				.Returns( new VariableOutputProcBResult
				{
					Id = 5,
					TextMessage = TEXT_MESSAGE
				} );

			dataMock.Execute( Settings.Default.DataMockConnectionString );

			var dataContext = new DataMockDataContext();

			var result = dataContext.VariableOutputProcA( 3 ).ToArray();

			Assert.AreEqual( 5, result[0].Id );
			Assert.AreEqual( 5, result[1].Id );
			Assert.AreEqual( 5, result[2].Id );

		}

		[TestMethod]
		public void VariableOutputProcedureReplacementWithSetParam()
		{
			var dataMock = new DataMock<DataMockDataContext>
			{
				Log = Console.Out
			};

			const string TEXT_MESSAGE = "This is a test and nothing but a test";

			dataMock.Setup( dc => dc.VariableOutputProcB( 1 ) )
				.Returns( new VariableOutputProcBResult
				{
					Id = 10,
					TextMessage = TEXT_MESSAGE
				} );

			dataMock.Setup( dc => dc.VariableOutputProcB( 2 ) )
				.Returns( new VariableOutputProcBResult
				{
					Id = 20,
					TextMessage = TEXT_MESSAGE
				} );

			dataMock.Setup( dc => dc.VariableOutputProcB( 3 ) )
				.Returns( new VariableOutputProcBResult
				{
					Id = 30,
					TextMessage = TEXT_MESSAGE
				} );

			dataMock.Execute( Settings.Default.DataMockConnectionString );

			var dataContext = new DataMockDataContext();

			var result = dataContext.VariableOutputProcA( 3 ).ToArray();

			Assert.AreEqual( 10, result[0].Id );
			Assert.AreEqual( 20, result[1].Id );
			Assert.AreEqual( 30, result[2].Id );

		}

		#endregion SprocTests Members

	}
}
