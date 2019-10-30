using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Mark5.Mobile.Common.Model;

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
        const string RecipientRegex = @"([^,])([^,]*)";

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
                return false;

            return Regex.Match(hostName, OnlyIpAddressRegex).Success || Regex.Match(hostName, OnlyHostnameRegex).Success;
        }

        public static bool IsPortValid(string port)
        {
            if (string.IsNullOrEmpty(port))
                return false;

            return Regex.Match(port, OnlyPortRegex).Success && IsPortValid(int.Parse(port));
        }

        public static bool IsPortValid(int port)
        {
            return port > 0 && port < 65536;
        }

        public static bool IsEmailValid(string email)
        {
            if (string.IsNullOrEmpty(email))
                return false;

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

        public static List<DocumentAddress> ExtractValidaEmails(string text)
        {
            List<DocumentAddress> addresses = new List<DocumentAddress>();

            var entries = text.Split(',');

            foreach (var entry in entries)
            {
                var validEmail = Regex.Matches(entry ?? string.Empty, EmailAddressRegex, RegexOptions.IgnoreCase);
                if (validEmail.Count > 0)
                {
                    var email = validEmail[0].Value;
                    var entrySplit = entry.Split('<');

                    addresses.Add(new DocumentAddress
                    {
                        Address = email,
                        Name = entrySplit.Count() > 1 ? entrySplit[0].Trim() : string.Empty,
                        Type = CommunicationAddressType.Email
                    });
                }
            }

            return addresses;
        }

        public static bool ContainsValidEmails(string text)
        {
            return ContainsValidEmails(text, out List<DocumentAddress> addresses);
        }

        public static bool ContainsValidEmails(string text, out List<DocumentAddress> addresses)
        {
            addresses = ExtractValidaEmails(text);
            return addresses.Count > 0;
        }

        public static bool ContainsValidEmail(string text)
        {
            return !string.IsNullOrEmpty(text) && Regex.IsMatch(text, EmailAddressRegex, RegexOptions.IgnoreCase);
        }

        public static bool ContainsValidTelephoneNumber(string text, out string phoneNumber)
        {
            phoneNumber = new string(text.ToCharArray().Where(c => char.IsDigit(c) || c == '+').ToArray());
            return !string.IsNullOrEmpty(phoneNumber);
        }

        public static bool ContainsValidUsernames(string text, SystemUsersDepartments systemUserDepartments)
        {
            return ContainsValidUsernames(text, systemUserDepartments, out IEnumerable<Match> mc);
        }

        public static bool ContainsValidUsernames(string text, SystemUsersDepartments systemUserDepartments, out IEnumerable<Match> matches)
        {
            matches = ExtractUsernames(text, systemUserDepartments);
            return matches.Any();
        }

        public static IEnumerable<Match> ExtractUsernames(string text, SystemUsersDepartments systemUsersDepartments)
        {
            var matches = Regex.Matches(text ?? string.Empty, RecipientRegex, RegexOptions.IgnoreCase).Cast<Match>();

            if (systemUsersDepartments != null)
                matches = matches.Cast<Match>().Where(m => !m.Value.Contains('@') && systemUsersDepartments.Users.Any(su => String.Equals(su.Username, m.Value.Trim(), StringComparison.OrdinalIgnoreCase))).Select(m => m);

            return matches;
        }
    }
}