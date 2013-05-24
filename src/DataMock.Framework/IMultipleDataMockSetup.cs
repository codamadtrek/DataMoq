namespace LazyE9.DataMock
{
    public interface IMultipleDataMockSetup<in TResult>
    {
        #region IMultipleDataMockSetup Members

        void Returns(params TResult[] results);

        #endregion IMultipleDataMockSetup Members

    }
}