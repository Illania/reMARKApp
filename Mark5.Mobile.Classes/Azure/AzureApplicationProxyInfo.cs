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
            return $"AppClientId = {AppClientId}, ApplicationProxyClientId = {ApplicationProxyClientId}, IsEnabled = {IsEnabled}";
        }

        /// <summary>
        /// Checks if Proxy is Enabled and Client IDs are filled
        /// </summary>
        /// <returns>
        /// true - if Proxy is Enabled and Client IDs are filed
        /// false - in all other cases
        /// </returns>
        public bool IsValid()
        {
            if (IsEnabled && !string.IsNullOrEmpty(AppClientId) && !string.IsNullOrEmpty(ApplicationProxyClientId))
                return true;
            else
                return false;
        }

    }
}
