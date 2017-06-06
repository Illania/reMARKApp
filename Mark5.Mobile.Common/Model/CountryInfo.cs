namespace Mark5.Mobile.Common.Model
{
    public class CountryInfo
    {
        public int Id { get; set; } = -1;

        public int FaxPrefix { get; set; } = -1;

        public int TelexPrefix { get; set; } = -1;

        public string CCode { get; set; }
        public string CCode3 { get; set; }
        public string Name { get; set; }
    }
}