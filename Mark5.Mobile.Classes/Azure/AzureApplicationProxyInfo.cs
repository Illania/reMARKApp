using System;

namespace Mark5.Mobile.Classes.Azure
{
    public class AzureApplicationProxyInfo
    {
        public string AppClientId { get; set; } = "";
        public string ApplicationProxyClientId { get; set; } = "";
        public bool IsEnabled { get; set; } = false;

        public override string ToString()
        {
            return $"AppClientId = {AppClientId}, ApplicationProxyCClientId = {ApplicationProxyClientId}, IsEnabled = {IsEnabled}";
        }
    }
}
