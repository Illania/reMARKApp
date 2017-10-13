using System;
namespace Mark5.Mobile.Common.Model
{
    public class ContactPhoneNumber
    {
        public string Name { get; set; }
        public string Number { get; set; }

        public ContactPhoneNumber(string name, string number)
        {
            Name = name;
            Number = number;
        }

        public ContactPhoneNumber() { }

        #region Overrides

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != typeof(ContactPhoneNumber))
                return false;
            
            var other = (ContactPhoneNumber)obj;
            return string.Equals(Name, other.Name, StringComparison.CurrentCultureIgnoreCase) && string.Equals(Number, other.Number, StringComparison.CurrentCultureIgnoreCase);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Name != null ? Name.GetHashCode() : 0) ^ (Number != null ? Number.GetHashCode() : 0);
            }
        }

        //This is used by the SuggestionAdapter in Android for the text to be inserted
        public override string ToString()
        {
            return string.IsNullOrEmpty(Name) ? Number : string.Format("{0} <{1}>", Name, Number);
        }

        #endregion

        public static int SortingComparison(ContactPhoneNumber x, ContactPhoneNumber y)
        {
            if (Convert.ToInt32(x.Number) < Convert.ToInt32(y.Number))
            {
                return -1;
            } else {
                return 1;
            }
        }

        public static int LookupComparison(ContactPhoneNumber x, ContactPhoneNumber y)
        {
            return x.GetHashCode().CompareTo(y.GetHashCode());
        }
    }
    }
}
