using System.Collections.Generic;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Common.Utilities
{
    public interface IPhonebookUtils
    {
        List<Contact> GetPhonebookContacts();

        List<Contact> GetFilteredPhonebookContacts(string phrase);
    }
}