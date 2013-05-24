namespace LazyE9.DataMock
{
    public interface ISingleDataMockSetup<in TResult>
    {
        #region ISingleDataMockSetup Members

        void Returns();

        void Returns(TResult result);

        #endregion ISingleDataMockSetup Members

    }
}