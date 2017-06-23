using System;
using System.Collections.Generic;
using System.Threading;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Common.Services
{
    public interface ISuggestionsRetrievalService
    {
        void GetSuggestions(string phrase, CancellationToken token, Action<List<Recipient>, CancellationToken> handler);
    }
}