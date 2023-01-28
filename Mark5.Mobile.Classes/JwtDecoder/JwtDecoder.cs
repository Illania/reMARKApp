using System;
using Newtonsoft.Json;

namespace Mark5.Mobile.Classes.JwtDecoder
{
    /// <summary>
    /// Sometimes all you need is a simple decoder.
    /// </summary>
    public static class Decoder
    {
        /// <summary>
        /// Decode the specified token.
        /// </summary>
        /// <returns>A tupal contained the decoded Header and Payload.</returns>
        /// <param name="token">The token you with to decode.</param>
        public static (JwtHeader Header, string Payload, string Verification) DecodeToken(string token)
        {
            string[] split = token.Split('.');
            if (split.Length > 1)
            {
                JwtHeader jsonHeaderData = JsonConvert.DeserializeObject<JwtHeader>(Base64DecodeToString(split[0]));

                string jsonData = Base64DecodeToString(split[1]);

                //byte[] verficationBytes = EncodingHelper.GetBytes(Base64DecodeToString(split[2]));
                string verification = split[2];

                return (jsonHeaderData, jsonData, verification);
            }
            else
            {
                throw new InvalidTokenPartsException("token");
            }
        }

        /// <summary>
        /// Decodes the payload into the provided type.
        /// </summary>
        /// <returns>The payload.</returns>
        /// <param name="token">A properly formatted .</param>
        /// <typeparam name="T">The type you wish to decode into.</typeparam>
        public static T DecodePayload<T>(string token)
        {
            var payloadDecoded = JsonConvert.DeserializeObject<T>(DecodeToken(token).Payload);
            return payloadDecoded;
        }

        private static string Base64DecodeToString(string ToDecode)
        {
            string decodePrepped = ToDecode.Replace("-", "+").Replace("_", "/");

            switch (decodePrepped.Length % 4)
            {
                case 0:
                    break;
                case 2:
                    decodePrepped += "==";
                    break;
                case 3:
                    decodePrepped += "=";
                    break;
                default:
                    throw new Exception("Not a legal base64 string!");
            }

            byte[] data = Convert.FromBase64String(decodePrepped);
            return System.Text.Encoding.UTF8.GetString(data);
        }

        /// <summary>
        /// Is the token expired?
        /// </summary>
        /// <returns><c>true</c>, if expired, <c>false</c> otherwise.</returns>
        /// <param name="token">Token.</param>
        public static bool IsCloseToExpire(string token)
        {
            var (_, Payload, _) = DecodeToken(token);
            var expiration = JsonConvert.DeserializeObject<JwtExpiration>(Payload).ExpiresAt;

            bool isCloseToExpire = expiration != null;


            //We consider a token close to expire if it is going to expire in 10 minutes
            if (expiration != null)
            {
                isCloseToExpire = DateTimeHelpers.FromUnixTime((long)expiration) < DateTime.UtcNow.AddMinutes(10);
            }

            return isCloseToExpire;
        }


        public static string GetUserInfo(string token)
        {
            var description = "";
            var (_, Payload, _) = DecodeToken(token);

            var userInfo = JsonConvert.DeserializeObject<JwtUserInfo>(Payload);
            var upn = userInfo.UserPrincipalInfo;
            var name = userInfo.UserName;
            description += $"User name: {name} \n";
            description += $"Email: {upn} \n";

            var expInfo = JsonConvert.DeserializeObject<JwtExpiration>(Payload);
            var issuedAt = expInfo.IssuedAt;
            if (issuedAt != null)
            {
                var issueDate = DateTimeHelpers.FromUnixTime((long)issuedAt);
                description += $"Token issued on: {issueDate.ToUniversalTime().ToString("G")} UTC\n";
            }

            var expiresAt = expInfo.ExpiresAt;
            if (expiresAt != null)
            {
                var expDate = DateTimeHelpers.FromUnixTime((long)expiresAt);
                description += $"Token expires on: {expDate.ToUniversalTime().ToString("G")} UTC\n";
            }

            return description;
        }
    }

    /// <summary>
    /// Exception thrown when when a token does not consist of three parts delimited by dots (".").
    /// </summary>
    public class InvalidTokenPartsException : ArgumentOutOfRangeException
    {
        /// <summary>
        /// Creates an instance of <see cref="InvalidTokenPartsException" />
        /// </summary>
        /// <param name="paramName">The name of the parameter that caused the exception</param>
        public InvalidTokenPartsException(string paramName)
            : base(paramName, "Token must consist of 3 delimited by dot parts.")
        {
        }
    }
}