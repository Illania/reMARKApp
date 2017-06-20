using System;
using System.Collections.Generic;

namespace Mark5.Mobile.Common.Model
{
    public class Recipient
    {
        public string Name { get; set; }
        public string ContactDescription { get; set; }
        public string Address { get; set; }
        public string AddressDescription { get; set; }
        public string ShortId { get; set; }
        public RecipientType Type { get; set; }

        public Recipient(RecentAddress ra)
            : this(ra.Name, ra.Address, RecipientType.RecentAddress)
        {
        }

        public Recipient()
        {
        }

        public Recipient(string name, string address, RecipientType type)
        {
            Name = name;
            Address = address;
            Type = type;
        }

        public static List<Recipient> GetPrintableSuggestionsFromContacts(List<Contact> contacts, RecipientType type)
        {
            var suggestions = new List<Recipient>();
            foreach (var contact in contacts)
                foreach (var address in contact.CommunicationAddresses)
                {
                    var fullName = contact.GetFullName();
                    suggestions.Add(new Recipient(fullName, address.Address, type));
                }

            return suggestions;
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
            return string.IsNullOrEmpty(Name) ? Address : string.Format("{0} <{1}>", Name, Address);
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