//
// Project: Mark5.Mobile.Common
// File: ISearchManager.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System.Threading.Tasks;
using Mark5.Mobile.Common.Model;
using System.Collections.Generic;

namespace Mark5.Mobile.Common.Managers
{

    public interface ISearchManager
    {

        Task<List<SavedSearch>> GetSavedSearches(SourceType sourceType = SourceType.Auto);

        Task<SearchDocumentsResult> SearchDocumentsAsync(SearchDocumentsCriteria criteria, SourceType sourceType = SourceType.Auto);

        Task<SearchContactsResult> SearchContactsAsync(SearchContactsCriteria critera, SourceType sourceType = SourceType.Auto);

        Task<SearchShortcodesResult> SearchShortcodesAsync(SearchShortcodesCriteria criteria, SourceType sourceType = SourceType.Auto);

        Task<Document> GetDocumentAsync(int searchId, int documentId, DocumentBodyTypeRequest bodyType, SourceType sourceType = SourceType.Auto);

        Task<Contact> GetContactAsync(int searchId, int contactId, SourceType sourceType = SourceType.Auto);

        Task<Shortcode> GetShortcodeAsync(int searchId, int shortcodeId, SourceType sourceType = SourceType.Auto);

    }
}

