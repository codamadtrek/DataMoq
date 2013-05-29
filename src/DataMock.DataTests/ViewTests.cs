using System;
using System.Linq;

using DataMock.DataTests.DataObjects;
using DataMock.DataTests.Properties;

using LazyE9.DataMock;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataMock.DataTests
{
	[TestClass]
	public class ViewTests : TestBase
	{
		#region ViewTests Members

		[TestMethod]
		public void MockViewWithMultipleSetupCalls()
		{
			var dataMock = new DataMock<DataMockDataContext>();

			const string MARY = "Mary";
			const string TRACY = "Tracy";
			const string CATHERINE = "Catherine";
			const string JOE = "Joe";
			const string JOHN = "John";

			var parents = new[] { MARY, TRACY };
			var children = new [] { JOHN, JOE, CATHERINE };

			dataMock
                .Setup( context => context.Peoples )
                .Returns(
				    new People
				    {
					    Child = CATHERINE,
					    Parent = MARY
				    },
				    new People
				    {
					    Child = JOE,
					    Parent = MARY
				    },
				    new People
				    {
					    Child = JOHN,
					    Parent = MARY
				    } );

			dataMock
                .Setup( context => context.Peoples )
                .Returns(
				    new People
				    {
					    Child = CATHERINE,
					    Parent = TRACY
				    },
				    new People
				    {
					    Child = JOE,
					    Parent = TRACY
				    },
				    new People
				    {
					    Child = JOHN,
					    Parent = TRACY
				    } );
			dataMock.Execute(Settings.Default.DataMockConnectionString);

			People[] people;
			using( var context = new DataMockDataContext() )
			{
				people = context.Peoples
					.OrderBy( person => person.Parent )
					.ThenBy( person => person.Child )
					.ToArray();
			}

			Assert.AreEqual( 6, people.Length );
			int counter = 0;
			foreach( string parent in parents.OrderBy( a => a ) )
			{
				foreach( var child in children.OrderBy( a => a ) )
				{
					People person = people[counter++];
					Assert.AreEqual(parent, person.Parent);
					Assert.AreEqual(child, person.Child);
				}
			}
		}

		[TestMethod]
		public void MockViewWithOneRow()
		{
            var dataMock = new DataMock<DataMockDataContext>
			{
				Log = Console.Out
			};
			const string CHILD = "Robert";
			const string PARENT = "Maude";
			dataMock
                .Setup( context => context.Peoples )
                .Returns( new People
			    {
				    Child = CHILD,
				    Parent = PARENT
			    } );
			dataMock.Execute( Settings.Default.DataMockConnectionString );

			using( var context = new DataMockDataContext() )
			{
				People person = context.Peoples.Single();
				Assert.IsTrue( person.Child == CHILD );
				Assert.IsTrue( person.Parent == PARENT );
			}
		}

		[TestMethod]
		public void MockViewWithSeveralRows()
		{
			var dataMock = new DataMock<DataMockDataContext>
			{
				Log = Console.Out
			};

			dataMock
                .Setup( context => context.Peoples )
                .Returns( new People
			    {
				    Child = "Owen",
				    Parent = "Michelle"
			    },
			    new People
			    {
				    Child = "Owen",
				    Parent = "Jacob"
			    } );
			dataMock.Execute( Settings.Default.DataMockConnectionString );

			using( var context = new DataMockDataContext() )
			{
				Assert.IsTrue( context.Peoples.Count() == 2 );
			}
		}

		#endregion ViewTests Members

	}
}
