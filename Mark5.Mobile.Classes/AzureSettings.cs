using System;
using Mark5.Mobile.Classes.JwtDecoder;
using Newtonsoft.Json;

namespace Mark5.Mobile.Classes

{
    public static class AzureSettings
    {
        public static string AccessToken { get; set; } = string.Empty;
        public static DateTime AccessTokenLastUpdated { get; set; } = DateTime.MinValue;
        public static string AppClientId { get; set; } = string.Empty;
        public static string AppProxyId { get; set; } = string.Empty;
        public static bool IsEnabled { get; set; } = false;

        public static string GetInfo()
        {
            DateTimeOffset accessTokenExpires = DateTimeOffset.MinValue;
            if (!string.IsNullOrEmpty(AccessToken))
            {
                var jwtAccessToken = Decoder.DecodeToken(AccessToken);
                accessTokenExpires = DateTimeOffset.FromUnixTimeSeconds((long)JsonConvert
                .DeserializeObject<JwtExpiration>(jwtAccessToken.Payload).ExpiresAt).ToLocalTime();
            }

            return $"AccessToken expires ={accessTokenExpires:f} " +
                $"AcccessToken last updated on={AccessTokenLastUpdated:f} " +
                $"AppClientId={AppClientId} " +
                $"AppProxyId={AppProxyId} " +
                $"IsEnabled={IsEnabled}";
        }

        public static bool IsTokenCloseToExpire()
        {
            if(!string.IsNullOrEmpty(AccessToken))
                return Decoder.IsCloseToExpire(AccessToken);

            else
                return false;
        }

    }
}
