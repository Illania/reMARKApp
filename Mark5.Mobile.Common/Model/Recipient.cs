using System;
using System.Collections.Generic;

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

        public Recipient(RecentAddress ra)
            : this(ra.Name, ra.Address, RecipientType.RecentAddress, ra.Id)
        {
        }

        public Recipient()
        {
        }

        public Recipient(string name, string address, RecipientType type, int id = 0)
        {
            Name = name;
            Address = address;
            Type = type;
            Id = id;
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
            return string.Equals(Name, other.Name, StringComparison.CurrentCultureIgnoreCase) && string.Equals(Address, other.Address, StringComparison.CurrentCultureIgnoreCase) && Type == other.Type;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Name != null ? Name.GetHashCode() : 0) ^ (Address != null ? Address.GetHashCode() : 0) ^ Type.GetHashCode();
            }
        }

        //This is used by the SuggestionAdapter in Android for the text to be inserted
        public override string ToString()
        {
            return Type == RecipientType.Internal ? Address : string.IsNullOrEmpty(Name) ? Address : $"{Name} <{Address}>";
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