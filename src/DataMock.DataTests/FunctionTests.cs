﻿using System;
using System.Linq;

using DataMock.DataTests.DataObjects;
using DataMock.DataTests.Properties;

using LazyE9.DataMock;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataMock.DataTests
{
	[TestClass]
	public class FunctionTests : TestBase
	{
		#region FunctionTests Members

		[TestMethod]
		public void MockFunctionWithExactParameters()
		{
			var dataMock = new DataMock<DataMockDataContext>
			{
				Log = Console.Out
			};

			const int QUERY_INT = 10;
			const string QUERY_STRING = "TEST";
			var queryGuid = Guid.NewGuid();
			var queryDate = DateTime.Today;

			const int RESULT_INT = 1;
			const string RESULT_STRING = "12345";
			var resultGuid = Guid.Empty;
			var resultDate = new DateTime(2012,1,1);
			
			dataMock
				.Setup( context => context.DataTypesFunction( QUERY_INT, QUERY_STRING, queryGuid, queryDate ) )
				.Returns( new DataTypesFunctionResult
				{
					c1 = RESULT_INT,
					c2 = RESULT_STRING,
					c3 = resultGuid,
					c4 = resultDate
				} );
			dataMock.Execute( Settings.Default.DataMockConnectionString );

			using( var context = new DataMockDataContext( Settings.Default.DataMockConnectionString ) )
			{
				DataTypesFunctionResult result = context.DataTypesFunction( QUERY_INT, QUERY_STRING, queryGuid, queryDate ).Single();
				Assert.AreEqual( RESULT_INT, result.c1 );
				Assert.AreEqual( RESULT_STRING, result.c2 );
				Assert.AreEqual( resultGuid, result.c3 );
				Assert.AreEqual( resultDate, result.c4 );
			}
		}

		[TestMethod]
		public void MockFunctionWithNewGuidExpression()
		{
			var dataMock = new DataMock<DataMockDataContext>
			{
				Log = Console.Out
			};

			const int QUERY_INT = 10;
			const string QUERY_STRING = "TEST";
			var queryGuid = new Guid( "F255039E-7809-E211-BDD9-08002704F29D" );

			const int RESULT_INT = 1;
			const string RESULT_STRING = "12345";
			var resultGuid = Guid.Empty;
			
			dataMock
				.Setup( context => context.DataTypesFunction( QUERY_INT, QUERY_STRING, new Guid( "F255039E-7809-E211-BDD9-08002704F29D" ), DateTime.Today))
				.Returns( new DataTypesFunctionResult
				{
					c1 = RESULT_INT,
					c2 = RESULT_STRING,
					c3 = resultGuid
				} );
			dataMock.Execute( Settings.Default.DataMockConnectionString );

			using( var context = new DataMockDataContext( Settings.Default.DataMockConnectionString ) )
			{
				DataTypesFunctionResult result = context.DataTypesFunction( QUERY_INT, QUERY_STRING, queryGuid, DateTime.Today ).Single();
				Assert.AreEqual( RESULT_INT, result.c1 );
				Assert.AreEqual( RESULT_STRING, result.c2 );
				Assert.AreEqual( resultGuid, result.c3 );
			}
		}

		#endregion FunctionTests Members

	}
}