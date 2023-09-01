using System;

namespace Mark5.Mobile.Common.Model
{

    public class SavedDocumentsSearch: IComparable<SavedDocumentsSearch>
    {
        public string Name { get; set; }
        public int Id { get; set; }
        public SearchDocumentsCriteria Criteria{ get; set; }

        public int CompareTo(SavedDocumentsSearch other)
        {
            if (other == null)
                return 1;

            else
                return Name.CompareTo(other.Name);
        }

        public bool Equals(SavedDocumentsSearch other)
        {
            if (other is null) return false;
            else return Id.Equals(other.Id) && Name.Equals(other.Name);
        }
    }

    public class SavedContactsSearch: IComparable<SavedContactsSearch>
    {
        public string Name { get; set; }
        public int Id { get; set; }
        public SearchContactsCriteria Criteria { get; set; }

        public int CompareTo(SavedContactsSearch other)
        {
            if (other == null)
                return 1;

            else
                return Name.CompareTo(other.Name);
        }

        public bool Equals(SavedContactsSearch other)
        {
            if (other is null) return false;
            else return Id.Equals(other.Id) && Name.Equals(other.Name);
        }
    }

    public class SavedShortcodesSearch: IComparable<SavedShortcodesSearch>
    {
        public string Name { get; set; }
        public int Id { get; set; }
        public SearchShortcodesCriteria Criteria { get; set; }

        public int CompareTo(SavedShortcodesSearch other)
        {
            if (other == null)
                return 1;

            else
                return Name.CompareTo(other.Name);
        }

        public bool Equals(SavedShortcodesSearch other)
        {
            if (other is null) return false;
            else return Id.Equals(other.Id) && Name.Equals(other.Name);
        }
    }
}