using System.Collections.Generic;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Common.Utilities
{
    public interface IPhonebookUtils
    {
        List<PrintableSuggestion> GetPhonebookContacts();

        List<PrintableSuggestion> GetFilteredPhonebookContacts(string phrase);
    }
}