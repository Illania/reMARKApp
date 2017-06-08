using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Common.Services
{
    public class SuggestionsRetrievalService
    {
        public void GetSuggestions(string phrase, CancellationToken token, Action<List<PrintableSuggestion>, CancellationToken> handler)
        {
            if (token.IsCancellationRequested)
                return;

            GetSuggestionFromRecentAddresses(phrase, token, handler);
            GetSuggestionFromContacts(phrase, token, handler);
            GetSuggestionFromPhonebook(phrase, token, handler);
        }

        public void GetSuggestionFromRecentAddresses(string phrase, CancellationToken token, Action<List<PrintableSuggestion>, CancellationToken> handler)
        {
            if (token.IsCancellationRequested)
                return;

            Task.Run(async () =>
            {
                var filtered = new List<PrintableSuggestion>();
                try
                {
                    var recentAddresses = await Managers.Managers.DocumentsManager.GetRecentAddressesAsync();
                    filtered = recentAddresses.Where(r => r.Address.ContainsCaseInsensitive(phrase) || r.Name.ContainsCaseInsensitive(phrase)).Select(ra => new PrintableSuggestion(ra)).ToList();
                }
                catch (Exception ex)
                {
                    CommonConfig.Logger.Error("Error while retrieving recent addresses", ex);
                }
                handler(filtered, token);
            });
        }

        public void GetSuggestionFromPhonebook(string phrase, CancellationToken token, Action<List<PrintableSuggestion>, CancellationToken> handler)
        {
            if (token.IsCancellationRequested)
                return;

            Task.Run(() =>
            {
                var phonebookContacts = CommonConfig.PhonebookUtilities.GetFilteredPhonebookContacts(phrase) ?? new List<PrintableSuggestion>();
                handler(phonebookContacts, token);
            });
        }

        public void GetSuggestionFromContacts(string phrase, CancellationToken token, Action<List<PrintableSuggestion>, CancellationToken> handler)
        {
            if (token.IsCancellationRequested)
                return;

            Task.Run(async () =>
            {
                var filtered = new List<PrintableSuggestion>();
                try
                {
                    filtered = await Managers.Managers.ContactsManager.GetSuggestions(phrase);
                }
                catch (Exception ex)
                {
                    CommonConfig.Logger.Error("Error while retrieving suggestions from database", ex);
                }
                handler(filtered, token);
            });
        }
    }
}