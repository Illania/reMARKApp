//
// Project: Mark5.Mobile.Common
// File: SuggestionService.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Managers;
using System.Linq;
using System.Threading;

namespace Mark5.Mobile.Common.Utilities
{
    public class SuggestionsRetrievalService
    {
        public void GetSuggestions(string phrase, CancellationToken token, Action<List<PrintableSuggestion>, CancellationToken> handler)
        {
            GetSuggestionFromRecentAddresses(phrase, token, handler);
            GetSuggestionFromContacts(phrase, token, handler);
            GetSuggestionFromPhonebook(phrase, token, handler);
        }

        public void GetSuggestionFromRecentAddresses(string phrase, CancellationToken token, Action<List<PrintableSuggestion>, CancellationToken> handler)
        {
            Task.Run(async () =>
            {
                var filtered = new List<PrintableSuggestion>();
                try
                {
                    var recentAddresses = await Managers.Managers.DocumentsManager.GetRecentAddressesAsync(); //TODO eventually this should happen only the first time
                    filtered = recentAddresses.Where(r => r.Address.ContainsCaseInsensitive(phrase) || r.Name.ContainsCaseInsensitive(phrase)) //TODO need to select the right type
                                                 .Select(ra => new PrintableSuggestion(ra)).ToList();
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
            var a1 = new PrintableSuggestion("Ferdinando", "fp@nordic-it.com", SuggestionType.Phonebook);
            var a2 = new PrintableSuggestion("Luigi", "ls@nordic-it.com", SuggestionType.Phonebook);
            var a3 = new PrintableSuggestion("Magda", "ma@nordic-it.com", SuggestionType.Phonebook);
            Task.Run(() =>
           {
               handler(new List<PrintableSuggestion> { a1, a2, a3 }, token);
           });
        }

        public void GetSuggestionFromContacts(string phrase, CancellationToken token, Action<List<PrintableSuggestion>, CancellationToken> handler)
        {
            var a4 = new PrintableSuggestion("Bartosz", "bgc@nordic-it.com", SuggestionType.Contact);
            var a5 = new PrintableSuggestion("", "fp@nordic-it.com", SuggestionType.Contact);
            Task.Run(() =>
           {
               handler(new List<PrintableSuggestion> { a4, a5 }, token);
           });
        }
    }
}
