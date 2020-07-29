namespace Mark5.Mobile.Common.Model.Azure
{
    public class AzureEndpointInfo
    {
        public string Name { get; set; }
        public string Hostname { get; set; }
        public int Port { get; set; }
        public SslMode SslMode { get; set; }
    }
}
