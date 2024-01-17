using System;
using Newtonsoft.Json;

namespace reMark.Mobile.Classes.JwtDecoder
{
    public class JwtHeader
    {
        [JsonProperty("alg")]
        public string Algorithm { get; set; }
        [JsonProperty("typ")]
        public string Type { get; set; }
    }

    public class JwtExpiration
    {
        [JsonProperty("iat")]
        public double? IssuedAt { get; set; }

        [JsonProperty("exp")]
        public double? ExpiresAt { get; set; }
    }

    public class JwtUserInfo
    {
        [JsonProperty("upn")]
        public string UserPrincipalInfo { get; set; }

        [JsonProperty("name")]
        public string UserName { get; set; }
    }
}
