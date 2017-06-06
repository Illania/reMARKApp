using System;
using System.Collections.Generic;

namespace Mark5.Mobile.Common.Model
{
    public class PrintableSuggestion
    {
        public string Name { get; set; }
        public string ContactDescription { get; set; }
        public string Address { get; set; }
        public string AddressDescription { get; set; }
        public string ShortId { get; set; }
        public SuggestionType Type { get; set; }

        public PrintableSuggestion(RecentAddress ra)
            : this(ra.Name, ra.Address, SuggestionType.RecentAddress)
        {
        }

        public PrintableSuggestion()
        {
        }

        public PrintableSuggestion(string name, string address, SuggestionType type)
        {
            Name = name;
            Address = address;
            Type = type;
        }

        public static List<PrintableSuggestion> GetPrintableSuggestionsFromContacts(List<Contact> contacts, SuggestionType type)
        {
            var suggestions = new List<PrintableSuggestion>();
            foreach (var contact in contacts)
            foreach (var address in contact.CommunicationAddresses)
            {
                var fullName = $"{contact.FirstName}{(string.IsNullOrEmpty(contact.Patronymic) ? string.Empty : " " + contact.Patronymic)}" + $"{(string.IsNullOrEmpty(contact.LastName) ? "" : " " + contact.LastName)}";
                suggestions.Add(new PrintableSuggestion(fullName, address.Address, type));
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
            if (obj.GetType() != typeof(PrintableSuggestion))
                return false;
            var other = (PrintableSuggestion) obj;
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

        public static int SortingComparison(PrintableSuggestion x, PrintableSuggestion y)
        {
            if (x.Type == SuggestionType.RecentAddress && y.Type != SuggestionType.RecentAddress)
                return -1;
            if (x.Type != SuggestionType.RecentAddress && y.Type == SuggestionType.RecentAddress)
                return 1;
            var nameX = string.IsNullOrEmpty(x.Name) ? x.Address : x.Name;
            var nameY = string.IsNullOrEmpty(y.Name) ? y.Address : y.Name;

            return string.Compare(nameX, nameY, StringComparison.CurrentCulture);
        }

        public static int LookupComparison(PrintableSuggestion x, PrintableSuggestion y)
        {
            return x.GetHashCode().CompareTo(y.GetHashCode());
        }
    }
}