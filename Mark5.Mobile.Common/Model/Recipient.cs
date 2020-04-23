using System;
using System.Collections.Generic;
using System.Linq;
using Mark5.Mobile.Common.Utilities;

namespace Mark5.Mobile.Common.Model
{
    public class Recipient
    {
        public int Id { get; set; } //Valid for contacts and internal
        public string Name { get; set; }
        public string ContactDescription { get; set; }
        public string Address { get; set; }
        public string AddressDescription { get; set; }
        public string ShortId { get; set; }
        public RecipientType Type { get; set; }

        public List<DocumentAddress> ShortcodeAddresses => Type == RecipientType.Shortcode 
            ? Serializer.Deserialize<List<DocumentAddress>>(Address) 
            : new List<DocumentAddress>();

        public Recipient(RecentAddress ra)
            : this(ra.Name, ra.Address, RecipientType.RecentAddress)
        {
        }

        public Recipient()
        {
        }

        public Recipient(string name, string address, RecipientType type, int id = -1)
        {
            Name = name;
            Address = address;
            Type = type;
            Id = id;
        }
        
        public string GetAddressPreviewText(DocumentAddressType addressType)
        {
            return Type == RecipientType.Shortcode 
                ? string.Join(", ",ShortcodeAddresses
                    .Where(a => a.AddressType == addressType)
                    .Select(a => a.Address).ToList()) 
                : Address;
        }

        public string GetFullAddressText(DocumentAddressType addressType)
        {
            if (Type == RecipientType.Shortcode) 
                return GetAddressPreviewText(addressType);
            
            return Type == RecipientType.Internal 
                ? Address 
                : string.IsNullOrEmpty(Name) ? Address : $"{Name} <{Address}>";
        }

        #region Overrides

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != typeof(Recipient))
                return false;

            var other = (Recipient)obj;
            return string.Equals(Name, other.Name, StringComparison.CurrentCultureIgnoreCase) && string.Equals(Address, 
                other.Address, StringComparison.CurrentCultureIgnoreCase) && Type == other.Type;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Name != null ? Name.GetHashCode() : 0) ^ (Address != null ? Address.GetHashCode() : 0) ^ Type.GetHashCode();
            }
        }

        #endregion

        public static int SortingComparison(Recipient x, Recipient y)
        {
            if (x.Type == RecipientType.RecentAddress && y.Type != RecipientType.RecentAddress)
                return -1;
            if (x.Type != RecipientType.RecentAddress && y.Type == RecipientType.RecentAddress)
                return 1;

            var nameX = string.IsNullOrEmpty(x.Name) ? x.Address : x.Name;
            var nameY = string.IsNullOrEmpty(y.Name) ? y.Address : y.Name;

            return string.Compare(nameX, nameY, StringComparison.CurrentCulture);
        }

        public static int LookupComparison(Recipient x, Recipient y)
        {
            return x.GetHashCode().CompareTo(y.GetHashCode());
        }
    }
}