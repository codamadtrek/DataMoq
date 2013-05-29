namespace LazyE9.DataMock
{
    public static class Param
    {
        #region Param Members

        public static TValue IsAny<TValue>()
        {
            return default(TValue);
        }

        #endregion Param Members

    }
}
