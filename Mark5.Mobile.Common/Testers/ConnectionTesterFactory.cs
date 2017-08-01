namespace Mark5.Mobile.Common.Testers
{
    public static class ConnectionTesterFactory
    {
        public static IConnectionTester Create()
        {
            return new ConnectionTester();
        }
    }
}