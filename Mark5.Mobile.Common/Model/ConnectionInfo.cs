namespace Mark5.Mobile.Common.Model
{
    public class ConnectionInfo
    {
        public string Token { get; set; }
        public string MicrosoftUserId { get; set; }
        public string Username { get; set; }
        public string Hostname { get; set; }
        public int Port { get; set; }
        public SslMode SslMode { get; set; }
        public DeviceType DeviceType { get; set; }
        public string FriendlyDeviceName { get; set; }
        public string InstallationId { get; set; }

        public override string ToString()
        {
            return $"[ConnectionInfo: Token={Token}, Username={Username}, Hostname={Hostname}, Port={Port}, " +
                $"SslMode={SslMode}, DeviceType={DeviceType}, FriendlyDeviceName={FriendlyDeviceName}, InstallationId={InstallationId}]";
        }
    }
}