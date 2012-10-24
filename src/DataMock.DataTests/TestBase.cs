using System;
using System.Collections.Generic;
using System.Text;
using System.Transactions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataMock.DataTests
{
	/// <summary>
	/// Summary description for UnitTest2
	/// </summary>
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
