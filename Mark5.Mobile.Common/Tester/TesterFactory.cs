namespace Mark5.Mobile.Common.Tester
{
    public static class TesterFactory
    {
        public static ITester Create()
        {
            return new Tester();
        }
    }
}