//
// Project: Mark5.Mobile.Common
// File: SearchManager.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Model.Converters;
using Mark5.ServiceReference.AppService;
using DataContract = Mark5.ServiceReference.DataContract;

namespace Mark5.Mobile.Common.Managers
{

    class SearchManager : AbstractManager, ISearchManager
    {

        public SearchManager(ConnectionInfo connectionInfo, IAppServiceProxy appServiceProxy)
                : base(connectionInfo, appServiceProxy)
        {
        }

        public async Task<List<SavedSearch>> GetSavedSearches(SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto || sourceType == SourceType.Remote)
            {
                var result = await AppServiceProxy.GetSavedSearchesAsync(new DataContract.GetSavedSearchesParameters
                {
                    Token = Token
                });

                return result.SavedSearches.WhereNotNull().OrderBy(ss => ss.Name).Select(ss => ss.Convert()).ToList();
            }

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task<SearchDocumentsResult> SearchDocumentsAsync(SearchDocumentsCriteria criteria, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto || sourceType == SourceType.Remote)
            {
                var result = await AppServiceProxy.SearchDocumentsAsync(new DataContract.SearchDocumentsParameters
                {
                    Token = Token,
                    SavedSearchFilterHash = criteria.SavedSearchFilterHash,
                    MaxToFetch = criteria.MaxToFetch,
                    SubjectMessageField = criteria.SubjectMessageField,
                    SubjectMessageClause = criteria.SubjectMessageClause.ConvertEnum<DataContract.SubjectMessageClause>(),
                    FromToField = criteria.FromToField,
                    FromToClause = criteria.FromToClause.ConvertEnum<DataContract.FromToClause>(),
                    SearchInAttachments = criteria.SearchInAttachments,
                    Unread = criteria.Unread,
                    PartialWordSearch = criteria.PartialWordSearch,
                    Processed = criteria.Processed,
                    Reference = criteria.Reference,
                    Priorities = criteria.Priorities.Select(p => p.ConvertEnum<DataContract.Priority>()).ToList(),
                    Directions = criteria.Directions.Select(p => p.ConvertEnum<DataContract.DocumentDirection>()).ToList(),
                    CategoryIds = criteria.CategoryIds.ToList(),
                    MustHaveCategoryIds = criteria.MustHaveCategoryIds.ToList(),
                    LineGuids = criteria.LineGuids.ToList(),
                    CreatorGuids = criteria.CreatorGuids.ToList(),
                    DateRange = {
                        Enabled = criteria.DateRange?.Enabled ?? false,
                        Start = criteria.DateRange?.Start ?? default(DateTime),
                        End = criteria.DateRange?.End ?? default(DateTime),
                    },
                    Comment = criteria.Comment,
                    AttachmentName = criteria.AttachmentName,
                    HavingAttachmentsOnly = criteria.HavingAttachmentsOnly,
                    FiledInFolderType = criteria.FiledInFolderType.ConvertEnum<DataContract.FiledInFolderType>(),
                    FiledInFolderFolderType = criteria.FiledInFolderFolderType.ConvertEnum<DataContract.FiledInFolderFolderType>(),
                    ExtraFields = criteria.ExtraFields
                });

                return new SearchDocumentsResult
                {
                    SearchId = result.SearchId,
                    DocumentPreviews = result.SearchResults.WhereNotNull().OrderByDescending(dp => dp.DateReceived).Select(dp => dp.Convert()).ToList()
                };
            }

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task<SearchContactsResult> SearchContactsAsync(SearchContactsCriteria criteria, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto || sourceType == SourceType.Remote)
            {
                var result = await AppServiceProxy.SearchContactsAsync(new DataContract.SearchContactsParameters
                {
                    Token = Token,
                    SavedSearchFilterHash = criteria.SavedSearchFilterHash,
                    MaxToFetch = criteria.MaxToFetch,
                    Name = criteria.Name,
                    FirstName = criteria.FirstName,
                    LastName = criteria.LastName,
                    ShortId = criteria.ShortId,
                    Description = criteria.Description,
                    ContactTypes = new HashSet<DataContract.ContactType>(criteria.ContactTypes.Select(ct => ct.ConvertEnum<DataContract.ContactType>())),
                    ComAddress = criteria.ComAddress,
                    PostAddress = criteria.PostAddress,
                    Vat = criteria.Vat,
                    Ledger = criteria.Ledger,
                    CountryPrefix = criteria.CountryPrefix,
                    CategoriesIds = criteria.CategoryIds.ToList(),
                    MustHaveCategoriesIds = criteria.MustHaveCategoryIds.ToList(),
                    Comment = criteria.Comment,
                    FiledInFolderType = criteria.FiledInFolderType.ConvertEnum<DataContract.FiledInFolderType>(),
                    FiledInFolderFolderType = criteria.FiledInFolderFolderType.ConvertEnum<DataContract.FiledInFolderFolderType>(),
                });

                return new SearchContactsResult
                {
                    SearchId = result.SearchId,
                    ContactPreviews = result.SearchResults.WhereNotNull().OrderBy(cp => cp.RowId).Select(cp => cp.Convert()).ToList()
                };
            }

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task<SearchShortcodesResult> SearchShortcodesAsync(SearchShortcodesCriteria criteria, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto || sourceType == SourceType.Remote)
            {
                var result = await AppServiceProxy.SearchShortcodesAsync(new DataContract.SearchShortcodesParameters
                {
                    Token = Token,
                    SavedSearchFilterHash = criteria.SavedSearchFilterHash,
                    MaxToFetch = criteria.MaxToFetch,
                    Name = criteria.Name,
                    Description = criteria.Description,
                    Address = criteria.Address,
                    FiledInFolderType = criteria.FiledInFolderType.ConvertEnum<DataContract.FiledInFolderType>(),
                    FiledInFolderFolderType = criteria.FiledInFolderFolderType.ConvertEnum<DataContract.FiledInFolderFolderType>(),
                });

                return new SearchShortcodesResult
                {
                    SearchId = result.SearchId,
                    ShortcodePreviews = result.ShortcodePreviews.WhereNotNull().OrderBy(sp => sp.RowId).Select(sp => sp.Convert()).ToList()
                };
            }

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task<Document> GetDocumentAsync(int searchId, int documentId, DocumentBodyTypeRequest bodyType, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto || sourceType == SourceType.Remote)
            {
                var result = await AppServiceProxy.GetDocumentAsync(new DataContract.GetDocumentParameters
                {
                    Token = Token,
                    FolderId = searchId,
                    DocumentId = documentId,
                    BodyRequest = bodyType.ConvertEnum<DataContract.DocumentBodyTypeRequest>(),
                    IncludePreview = false
                });

                return result.Document.Convert();
            }

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task<Contact> GetContactAsync(int searchId, int contactId, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto || sourceType == SourceType.Remote)
            {
                var result = await AppServiceProxy.GetContactAsync(new DataContract.GetContactParameters
                {
                    Token = Token,
                    FolderId = searchId,
                    ContactId = contactId,
                    IncludePreview = false
                });

                return result.Contact.Convert();
            }

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task<Shortcode> GetShortcodeAsync(int searchId, int shortcodeId, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto || sourceType == SourceType.Remote)
            {
                var result = await AppServiceProxy.GetShortcodeAsync(new DataContract.GetShortcodeParameters
                {
                    Token = Token,
                    FolderId = searchId,
                    ShortcodeId = shortcodeId,
                    IncludePreview = false
                });

                return result.Shortcode.Convert();
            }

            throw new ArgumentException("Invalid sourceType provided.");
        }
    }
}

