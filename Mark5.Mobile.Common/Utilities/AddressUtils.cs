using System;
using System.Linq;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Common.Utilities
{
    public static class AddressUtils
    {
        public static string FormatCommunicationAddress(CommunicationAddress ca)
        {
            if (ca.Address.Contains("|"))
                if (ca.Type == CommunicationAddressType.Mobile || ca.Type == CommunicationAddressType.Phone
                    || ca.Type == CommunicationAddressType.Fax || ca.Type == CommunicationAddressType.Telex)
                {
                    var addressParts = ca.Address.Split('|');
                    if (addressParts[0].Length > 0)
                        addressParts[0] = "+" + addressParts[0];
                    return string.Join(" ", addressParts.Where(s => !string.IsNullOrWhiteSpace(s)));
                }

            return ca.Address;
        }

        public static (string, string) CommunicationAddressParts(CommunicationAddress ca)
        {
            if (ca.Type == CommunicationAddressType.Mobile || ca.Type == CommunicationAddressType.Phone
                || ca.Type == CommunicationAddressType.Fax || ca.Type == CommunicationAddressType.Telex)
            {
                var addressParts = ca.Address.Split('|');
                return (addressParts[0], string.Join(" ", addressParts.Skip(1).Where(s => !string.IsNullOrWhiteSpace(s))));
            }

            throw new ArgumentException("This method can be used only with phone numbers!");
        }

        public static string FormatPhysicalAddress(PhysicalAddress pe)
        {
            var parts = new string[]
            {
                pe.Street,
                pe.Area,
                pe.City,
                pe.Country.Name
            }.Where(a => a != null && a.Any());
            return string.Join(", ", parts);
        }
    }
}