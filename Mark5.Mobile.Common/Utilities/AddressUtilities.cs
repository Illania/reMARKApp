//
// Project: Mark5.Mobile.Common
// File: CommnunicationAddressUtilities.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//
using System.Linq;
using System.Text;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Common.Utilities
{
    public static class AddressUtilities
    {
        public static string FormatCommunicationAddress(CommunicationAddress communicationAddress)
        {
            if (new[] { CommunicationAddressType.Fax, CommunicationAddressType.Phone, CommunicationAddressType.Mobile, CommunicationAddressType.Telex }.Contains(communicationAddress.Type))
            {
                var stringBuilder = new StringBuilder();
                var addressParts = communicationAddress.Address.Split('|');

                var countryPrefix = addressParts[0];
                var firstPart = addressParts[1];
                var secondPart = addressParts[2];

                if (!string.IsNullOrEmpty(countryPrefix))
                {
                    stringBuilder.Append($"+{countryPrefix} ");
                }
                if (!string.IsNullOrEmpty(firstPart))
                {
                    stringBuilder.Append($"{firstPart} ");
                }
                if (!string.IsNullOrEmpty(secondPart))
                {
                    stringBuilder.Append(secondPart);
                }

                return stringBuilder.ToString();
            }

            return communicationAddress.Address;

        }

        public static string FormatPhysicalAddress(PhysicalAddress pe)
        {
            var parts = new string[] { pe.Street, pe.Area, pe.City, pe.Country.Name }.Where(a => a != null && a.Any());
            return string.Join(", ", parts);
        }
    }
}
