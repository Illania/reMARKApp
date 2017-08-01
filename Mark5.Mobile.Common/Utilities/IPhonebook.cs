using System.Collections.Generic;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Common.Utilities
{
    public interface IPhonebook
    {
        List<Recipient> GetPhonebookContacts();

        List<Recipient> GetFilteredPhonebookContacts(string phrase);
    }
}