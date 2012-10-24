namespace LazyE9.DataMock
{
	public interface IDataMockSetup<in TResult>
	{
		#region IDataMockSetup Members

		void Returns( params TResult[] results );

		#endregion IDataMockSetup Members

	}
}