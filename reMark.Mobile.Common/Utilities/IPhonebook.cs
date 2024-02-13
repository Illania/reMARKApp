using System.Collections.Generic;
using reMark.Mobile.Common.Model;

namespace reMark.Mobile.Common.Utilities
{
    public interface IPhonebook
    {
        List<Recipient> GetPhonebookContacts();

        List<Recipient> GetFilteredPhonebookContacts(string phrase);
    }
}