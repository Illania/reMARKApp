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

namespace Mark5.Mobile.Common.Utilities
{
    public class SuggestionsRetrievalService
    {
        public async Task<List<PrintableSuggestion>> GetSuggestionFromRecentAddresses(string phrase)
        {
            try
            {
                var recentAddresses = await Managers.Managers.DocumentsManager.GetRecentAddressesAsync(); //TODO eventually this should happen only the first time
                var filtered = recentAddresses.Where(r => r.Address.ContainsCaseInsensitive(phrase) || r.Name.ContainsCaseInsensitive(phrase))
                                              .Select(ra => new PrintableSuggestion(ra)).ToList();
                return filtered;
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Error while retrieving recent addresses", ex);
                return new List<PrintableSuggestion>();
            }
        }

        public async Task<List<PrintableSuggestion>> GetSuggestionFromPhonebook()
        {
            var a1 = new PrintableSuggestion("Ferdinando", "fp@nordic-it.com", SuggestionType.Phonebook);
            var a2 = new PrintableSuggestion("Luigi", "ls@nordic-it.com", SuggestionType.Phonebook);
            var a3 = new PrintableSuggestion("Magda", "ma@nordic-it.com", SuggestionType.Phonebook);

            return new List<PrintableSuggestion> { a1, a2, a3 };
        }

        public async Task<List<PrintableSuggestion>> GetSuggestionFromContacts()
        {
            var a4 = new PrintableSuggestion("Bartosz", "bgc@nordic-it.com", SuggestionType.Contact);
            var a5 = new PrintableSuggestion("", "fp@nordic-it.com", SuggestionType.Contact);

            return new List<PrintableSuggestion> { a4, a5 };
        }
    }
}
