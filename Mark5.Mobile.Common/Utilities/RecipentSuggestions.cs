using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Common.Utilities
{
    public static class RecipentSuggestions
    {
        public static void GetSuggestions(string phrase, CancellationToken token, Action<List<Recipient>, CancellationToken> handler, bool includeInternalContacts = false)
        {
            if (token.IsCancellationRequested)
                return;

            GetSuggestionFromRecentAddresses(phrase, token, handler);
            GetSuggestionFromContacts(phrase, token, handler);
            if (includeInternalContacts)
                GetSuggestionFromInternalContacts(phrase, token, handler);
            GetSuggestionFromPhonebook(phrase, token, handler);
        }

        static void GetSuggestionFromRecentAddresses(string phrase, CancellationToken token, Action<List<Recipient>, CancellationToken> handler)
        {
            if (token.IsCancellationRequested)
                return;

            Task.Run(async () =>
            {
                var filtered = new List<Recipient>();
                try
                {
                    var recentAddresses = await Managers.DocumentsManager.GetRecentAddressesAsync(phrase.Length < 3 ? SourceType.Remote : SourceType.Local);
                    filtered = recentAddresses.Where(r => r.Address.ContainsCaseInsensitive(phrase) || r.Name.ContainsCaseInsensitive(phrase)).Select(ra => new Recipient(ra)).ToList();
                }
                catch (Exception ex)
                {
                    CommonConfig.Logger.Error("Error while retrieving recent addresses", ex);
                }
                handler(filtered, token);
            });
        }

        static void GetSuggestionFromPhonebook(string phrase, CancellationToken token, Action<List<Recipient>, CancellationToken> handler)
        {
            if (token.IsCancellationRequested)
                return;

            Task.Run(() =>
            {
                var phonebookContacts = CommonConfig.Phonebook.GetFilteredPhonebookContacts(phrase) ?? new List<Recipient>();
                handler(phonebookContacts, token);
            });
        }

        static void GetSuggestionFromContacts(string phrase, CancellationToken token, Action<List<Recipient>, CancellationToken> handler)
        {
            if (token.IsCancellationRequested)
                return;

            Task.Run(async () =>
            {
                var filtered = new List<Recipient>();
                try
                {
                    filtered = await Managers.ContactsManager.GetSuggestions(phrase);
                }
                catch (Exception ex)
                {
                    CommonConfig.Logger.Error("Error while retrieving suggestions from database", ex);
                }
                handler(filtered, token);
            });
        }

        static void GetSuggestionFromInternalContacts(string phrase, CancellationToken token, Action<List<Recipient>, CancellationToken> handler)
        {
            if (token.IsCancellationRequested)
                return;

            Task.Run(async () =>
            {
                var filtered = new List<Recipient>();

                var systemUsersDepartments = await Managers.SystemManager.GetSystemUsersDepartmentsAsync(SourceType.Local);

                var matchingInternalUsers = systemUsersDepartments.Users.FindAll(user => user.FirstName.ContainsCaseInsensitive(phrase) || user.LastName.ContainsCaseInsensitive(phrase) || user.Username.ContainsCaseInsensitive(phrase));

                foreach (SystemUser user in matchingInternalUsers)
                    filtered.Add(new Recipient((user.FirstName != null && user.LastName != null) ? user.FirstName + " " + user.LastName : null, user.Username, RecipientType.Internal));

                handler(filtered, token);
            });
        }
    }
}