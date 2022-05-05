using System;
using Mark5.Mobile.Classes.JwtDecoder;
using Newtonsoft.Json;

namespace Mark5.Mobile.Classes

{
    public static class AzureSettings
    {
        public static string AccessToken { get; set; } = string.Empty;
        public static string RefreshToken { get; set; } = string.Empty;
        public static DateTime RefreshTokenLastUpdated { get; set; } = DateTime.MinValue;
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
                .DeserializeObject<JwtExpiration>(jwtAccessToken.Payload).Expiration).ToLocalTime();
            }

            DateTimeOffset refreshTokenExpires = DateTimeOffset.MinValue;
            if (!string.IsNullOrEmpty(RefreshToken))
            {
                var jwtRefreshToken = Decoder.DecodeToken(RefreshToken);
                refreshTokenExpires = DateTimeOffset.FromUnixTimeSeconds((long)JsonConvert
                    .DeserializeObject<JwtExpiration>(jwtRefreshToken.Payload).Expiration).ToLocalTime();
            }
            

            return $"AccessToken expires ={accessTokenExpires:f} " +
                $"RefreshToken expires={refreshTokenExpires:f} " +
                $"RefreshToken last updated on={RefreshTokenLastUpdated:f} " +
                $"AppClientId={AppClientId} " +
                $"AppProxyId={AppProxyId} " +
                $"IsEnabled={IsEnabled}";
        }

        public static bool IsTokenExpired()
        {
            if(!string.IsNullOrEmpty(RefreshToken))
                return Decoder.IsExpired(RefreshToken);

            else if(!string.IsNullOrEmpty(AccessToken))
                return Decoder.IsExpired(AccessToken);

            else
                return false;
        }

    }
}
