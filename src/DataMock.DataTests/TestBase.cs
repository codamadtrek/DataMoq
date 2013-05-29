using System;
using System.Transactions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataMock.DataTests
{
	[TestClass]
	public class TestBase
	{
		#region TestBase Members

		[TestCleanup]
		public void CleanupTestTransaction()
		{
			if( mCurrentTestTransaction != null )
			{
				mCurrentTestTransaction.Dispose();
				mCurrentTestTransaction = null;
			}
		}

		[TestInitialize]
		public void InitializeTestTransaction()
		{
			mCurrentTestTransaction = new TransactionScope( TransactionScopeOption.RequiresNew, TimeSpan.FromMinutes( 2 ) );
		}

		#endregion TestBase Members

		#region Fields

		private TransactionScope mCurrentTestTransaction;

		#endregion Fields

	}
}
