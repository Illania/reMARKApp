//
// Project: Mark5.Mobile.Common
// File: Validator.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2014 Nordic IT
//
using System.Linq;
using System.Text.RegularExpressions;

namespace Mark5.Mobile.Common.Utilities
{

    public static class Validator
    {

        const string OnlyIpAddressRegex = @"^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$";
        const string OnlyHostnameRegex = @"^(([a-zA-Z0-9]|[a-zA-Z0-9][a-zA-Z0-9\-]*[a-zA-Z0-9])\.)*([A-Za-z0-9]|[A-Za-z0-9][A-Za-z0-9\-]*[A-Za-z0-9])$";
        const string OnlyPortRegex = @"^[0-9]{2,5}$";
        const string EmailAddressRegex = @"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?";
        const string OnlyEmailAddressRegex = @"^" + EmailAddressRegex + @"$";
        const string OnlyHexStringRegex = @"^#[A-Fa-f0-9]{0,2}[A-Fa-f0-9]{1,2}[A-Fa-f0-9]{1,2}[A-Fa-f0-9]{1,2}$";

        public static bool IsUsernameValid(string username)
        {
            return !string.IsNullOrEmpty(username);
        }

        public static bool IsPasswordValid(string password)
        {
            return !string.IsNullOrEmpty(password);
        }

        public static bool IsHostNameValid(string hostName)
        {
            if (string.IsNullOrEmpty(hostName))
            {
                return false;
            }
            return Regex.Match(hostName, OnlyIpAddressRegex).Success || Regex.Match(hostName, OnlyHostnameRegex).Success;
        }

        public static bool IsPortValid(string port)
        {
            if (string.IsNullOrEmpty(port))
            {
                return false;
            }

            return Regex.Match(port, OnlyPortRegex).Success && IsPortValid(int.Parse(port));
        }

        public static bool IsPortValid(int port)
        {
            return port > 0 && port < 65536;
        }

        public static bool IsEmailValid(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return false;
            }

            return Regex.Match(email, OnlyEmailAddressRegex, RegexOptions.IgnoreCase).Success;
        }

        public static bool IsHexStringValid(string hexString)
        {
            return hexString != null && Regex.Match(hexString, OnlyHexStringRegex).Success;
        }

        public static MatchCollection ExtractValidEmails(string text)
        {
            return Regex.Matches(text ?? string.Empty, EmailAddressRegex, RegexOptions.IgnoreCase);
        }

        public static bool ContainsValidEmails(string text)
        {
            MatchCollection mc;
            return ContainsValidEmails(text, out mc);
        }

        public static bool ContainsValidEmails(string text, out MatchCollection matches)
        {
            matches = ExtractValidEmails(text);
            return matches.Count > 0;
        }

        public static bool ContainsValidTelephoneNumber(string text, out string phoneNumber)
        {
            phoneNumber = new string(text.ToCharArray().Where(c => char.IsDigit(c) || c == '+').ToArray());
            return !string.IsNullOrEmpty(phoneNumber);
        }
    }
}
