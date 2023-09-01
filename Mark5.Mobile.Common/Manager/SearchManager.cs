using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mark5.Mobile.Common.DataAccess;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Model.Converters;
using Mark5.Mobile.Common.Model.Exceptions;
using Mark5.Mobile.Common.Storage;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Common.Utilities.Extensions;
using Mark5.ServiceReference.AppService;
using DataContract = Mark5.ServiceReference.DataContract;
using Mark5.Mobile.Classes.Enum;
using Mark5.ServiceReference.DataContract;


namespace Mark5.Mobile.Common.Manager
{
    class SearchManager : AbstractManager, ISearchManager
    {
        public Model.DocumentBodyTypeRequest DocumentBodyTypeRequest { get; set; } = Model.DocumentBodyTypeRequest.HtmlOnly;

        readonly IDocumentsDataAccess documentsDataAccess;
        readonly IContactsDataAccess contactsDataAccess;
        readonly IShortcodesDataAccess shortcodesDataAccess;

        public SearchManager(ConnectionInfo connectionInfo, IAppServiceProxy appServiceProxy,
                             IDocumentsDataAccess documentsDataAccess,
                             IContactsDataAccess contactsDataAccess,
                             IShortcodesDataAccess shortcodesDataAccess)
            : base(connectionInfo, appServiceProxy)
        {
            this.documentsDataAccess = documentsDataAccess;
            this.contactsDataAccess = contactsDataAccess;
            this.shortcodesDataAccess = shortcodesDataAccess;
        }

        public async Task<List<SavedDocumentsSearch>> GetSavedDocumentsSearchesAsync(SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.GetReachabilitySourceType();

            if (sourceType == SourceType.Remote)
            {
                var result = await AppServiceProxy.GetSavedDocumentsSearches(new DataContract.GetSavedDocumentsSearchesParameters
                {
                    Token = Token
                });

                return result.SavedSearches.WhereNotNull().Select(ss => ss.Convert()).ToList();
            }

            if (sourceType == SourceType.Local)
                throw new ReMarkException(ErrorConstants.Codes.InvalidSourceType);

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task<List<SavedContactsSearch>> GetSavedContactsSearchesAsync(SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.GetReachabilitySourceType();

            if (sourceType == SourceType.Remote)
            {
                var result = await AppServiceProxy.GetSavedContactsSearches(new DataContract.GetSavedContactsSearchesParameters
                {
                    Token = Token
                });

                return result.SavedSearches.WhereNotNull().Select(ss => ss.Convert()).ToList();
            }

            if (sourceType == SourceType.Local)
                throw new ReMarkException(ErrorConstants.Codes.InvalidSourceType);

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task<List<SavedShortcodesSearch>> GetSavedShortcodesSearchesAsync(SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.GetReachabilitySourceType();

            if (sourceType == SourceType.Remote)
            {
                var result = await AppServiceProxy.GetSavedShortcodesSearches(new DataContract.GetSavedShortcodesSearchesParameters
                {
                    Token = Token
                });

                return result.SavedSearches.WhereNotNull().Select(ss => ss.Convert()).ToList();
            }

            if (sourceType == SourceType.Local)
                throw new ReMarkException(ErrorConstants.Codes.InvalidSourceType);

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task DeleteSavedSearchAsync(int savedSearchId, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.GetReachabilitySourceType();

            if (sourceType == SourceType.Remote)
            {
                await AppServiceProxy.DeleteSavedSearch(new DataContract.DeleteSavedSearchesParameters
                {
                    Token = Token,
                    Id = savedSearchId
                });
                return;
            }

            if (sourceType == SourceType.Local)
                throw new ReMarkException(ErrorConstants.Codes.InvalidSourceType);

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task UpdateSavedDocumentsSearchAsync(int savedSearchId, SavedDocumentsSearch savedDocumentsSearch, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.GetReachabilitySourceType();

            if (sourceType == SourceType.Remote)
            {
                var criteria = savedDocumentsSearch.Criteria;
                var searchDocumentParameters = new SearchDocumentsParameters
                {
                    Token = Token,
                    SavedSearchFilterHash = criteria.SavedSearchFilterHash,
                    MaxToFetch = criteria.MaxToFetch,
                    SubjectMessageField = criteria.SubjectMessageField.SanitizeForSearch(),
                    SubjectMessageClause = criteria.SubjectMessageClause.ConvertEnum<DataContract.SubjectMessageClause>(),
                    FromToField = criteria.FromToField.SanitizeForSearch(),
                    FromToClause = criteria.FromToClause.ConvertEnum<DataContract.FromToClause>(),
                    SearchInAttachments = criteria.SearchInAttachments,
                    Unread = criteria.UnreadOnly,
                    PartialWordSearch = criteria.PartialWordSearch,
                    Processed = criteria.Handled,
                    Reference = criteria.Reference,
                    Priorities = criteria.Priorities.Select(p => p.ConvertEnum<DataContract.Priority>()).ToList(),
                    Directions = criteria.Directions.Select(p => p.ConvertEnum<DataContract.DocumentDirection>()).ToList(),
                    CategoryIds = criteria.CategoryIds.ToList(),
                    MustHaveCategoryIds = criteria.MustHaveCategoryIds.ToList(),
                    LineGuids = criteria.LineGuids.ToList(),
                    CreatorGuids = criteria.CreatorGuids.ToList(),
                    DateRange =
                    {
                        Enabled = criteria.DateRange?.Enabled ?? false,
                        Start = criteria.DateRange?.StartTimestamp.ConvertTimestampMillisecondsToDateTime() ?? default(DateTime),
                        End = criteria.DateRange?.EndTimestamp.ConvertTimestampMillisecondsToDateTime() ?? default(DateTime)
                    },
                    Comment = criteria.Comment,
                    AttachmentName = criteria.AttachmentName,
                    HavingAttachmentsOnly = criteria.HavingAttachmentsOnly,
                    FiledInFolderType = criteria.FiledInFolderType.ConvertEnum<DataContract.FiledInFolderType>(),
                    FiledInFolderFolderType = criteria.FiledInFolderFolderType.ConvertEnum<DataContract.FiledInFolderFolderType>(),
                    ExtraFields = criteria.ExtraFields
                };

                await AppServiceProxy.UpdateSavedSearch(new UpdateSavedSearchesParameters
                {
                    Token = Token,
                    Id = savedSearchId,
                    ModuleType = DataContract.ModuleType.Documents,
                    SavedSearchJson = Serializer.Serialize(searchDocumentParameters)
                });
                return;
            }

            if (sourceType == SourceType.Local)
                throw new ReMarkException(ErrorConstants.Codes.InvalidSourceType);

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task UpdateSavedContactsSearchAsync(int savedSearchId, SavedContactsSearch savedContactsSearch, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.GetReachabilitySourceType();

            if (sourceType == SourceType.Remote)
            {
                var criteria = savedContactsSearch.Criteria;
                var searchContactsParameters = new SearchContactsParameters
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
                    FiledInFolderFolderType = criteria.FiledInFolderFolderType.ConvertEnum<DataContract.FiledInFolderFolderType>()

                };

                await AppServiceProxy.UpdateSavedSearch(new UpdateSavedSearchesParameters
                {
                    Token = Token,
                    Id = savedSearchId,
                    ModuleType = DataContract.ModuleType.Contacts,
                    SavedSearchJson = Serializer.Serialize(searchContactsParameters)
                });
                return;
            }

            if (sourceType == SourceType.Local)
                throw new ReMarkException(ErrorConstants.Codes.InvalidSourceType);

            throw new ArgumentException("Invalid sourceType provided.");
        }
    

        public async Task UpdateSavedShortcodesSearchAsync(int savedSearchId, SavedShortcodesSearch savedShortcodesSearch, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.GetReachabilitySourceType();

            if (sourceType == SourceType.Remote)
            {
                var criteria = savedShortcodesSearch.Criteria;
                var searchShortcodesParameters = new SearchShortcodesParameters
                {
                    Token = Token,
                    SavedSearchFilterHash = criteria.SavedSearchFilterHash,
                    MaxToFetch = criteria.MaxToFetch,
                    Name = criteria.Name,
                    Description = criteria.Description,
                    Address = criteria.Address,
                    FiledInFolderType = criteria.FiledInFolderType.ConvertEnum<DataContract.FiledInFolderType>(),
                    FiledInFolderFolderType = criteria.FiledInFolderFolderType.ConvertEnum<DataContract.FiledInFolderFolderType>()

                };

                await AppServiceProxy.UpdateSavedSearch(new UpdateSavedSearchesParameters
                {
                    Token = Token,
                    Id = savedSearchId,
                    ModuleType = DataContract.ModuleType.Shortcodes,
                    SavedSearchJson = Serializer.Serialize(searchShortcodesParameters)
                });
                return;
            }

            if (sourceType == SourceType.Local)
                throw new ReMarkException(ErrorConstants.Codes.InvalidSourceType);

            throw new ArgumentException("Invalid sourceType provided.");

        }

        public async Task<SavedDocumentsSearch> AddSavedDocumentsSearchAsync(SavedDocumentsSearch savedDocumentsSearch, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.GetReachabilitySourceType();

            if (sourceType == SourceType.Remote)
            {

                var criteria = savedDocumentsSearch.Criteria;
                var searchDocumentParameters = new SearchDocumentsParameters
                {
                    Token = Token,
                    SavedSearchFilterHash = criteria.SavedSearchFilterHash,
                    MaxToFetch = criteria.MaxToFetch,
                    SubjectMessageField = criteria.SubjectMessageField.SanitizeForSearch(),
                    SubjectMessageClause = criteria.SubjectMessageClause.ConvertEnum<DataContract.SubjectMessageClause>(),
                    FromToField = criteria.FromToField.SanitizeForSearch(),
                    FromToClause = criteria.FromToClause.ConvertEnum<DataContract.FromToClause>(),
                    SearchInAttachments = criteria.SearchInAttachments,
                    Unread = criteria.UnreadOnly,
                    PartialWordSearch = criteria.PartialWordSearch,
                    Processed = criteria.Handled,
                    Reference = criteria.Reference,
                    Priorities = criteria.Priorities.Select(p => p.ConvertEnum<DataContract.Priority>()).ToList(),
                    Directions = criteria.Directions.Select(p => p.ConvertEnum<DataContract.DocumentDirection>()).ToList(),
                    CategoryIds = criteria.CategoryIds.ToList(),
                    MustHaveCategoryIds = criteria.MustHaveCategoryIds.ToList(),
                    LineGuids = criteria.LineGuids.ToList(),
                    CreatorGuids = criteria.CreatorGuids.ToList(),
                    DateRange =
                    {
                        Enabled = criteria.DateRange?.Enabled ?? false,
                        Start = criteria.DateRange?.StartTimestamp.ConvertTimestampMillisecondsToDateTime() ?? default(DateTime),
                        End = criteria.DateRange?.EndTimestamp.ConvertTimestampMillisecondsToDateTime() ?? default(DateTime)
                    },
                    Comment = criteria.Comment,
                    AttachmentName = criteria.AttachmentName,
                    HavingAttachmentsOnly = criteria.HavingAttachmentsOnly,
                    FiledInFolderType = criteria.FiledInFolderType.ConvertEnum<DataContract.FiledInFolderType>(),
                    FiledInFolderFolderType = criteria.FiledInFolderFolderType.ConvertEnum<DataContract.FiledInFolderFolderType>(),
                    ExtraFields = criteria.ExtraFields
                };

                var result = await AppServiceProxy.AddSavedSearch(new AddSavedSearchesParameters
                {
                    Token = Token,
                    ModuleType = DataContract.ModuleType.Documents,
                    Name = savedDocumentsSearch.Name,
                    SavedSearchJson = Serializer.Serialize(searchDocumentParameters)
                });

                return new SavedDocumentsSearch()
                {
                    Id = result.Id,
                    Criteria = criteria,
                    Name = savedDocumentsSearch.Name
                };

            }

            if (sourceType == SourceType.Local)
                throw new ReMarkException(ErrorConstants.Codes.InvalidSourceType);

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task<SavedContactsSearch> AddSavedContactsSearchAsync(SavedContactsSearch savedContactsSearch, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.GetReachabilitySourceType();

            if (sourceType == SourceType.Remote)
            {

                var criteria = savedContactsSearch.Criteria;
                var searchContactsParameters = new SearchContactsParameters
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
                    FiledInFolderFolderType = criteria.FiledInFolderFolderType.ConvertEnum<DataContract.FiledInFolderFolderType>()

                };

                var result = await AppServiceProxy.AddSavedSearch(new AddSavedSearchesParameters
                {
                    Token = Token,
                    ModuleType = DataContract.ModuleType.Contacts,
                    Name = savedContactsSearch.Name,
                    SavedSearchJson = Serializer.Serialize(searchContactsParameters)
                });

                return new SavedContactsSearch()
                {
                    Id = result.Id,
                    Criteria = criteria,
                    Name = savedContactsSearch.Name
                };

            }

            if (sourceType == SourceType.Local)
                throw new ReMarkException(ErrorConstants.Codes.InvalidSourceType);

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task<SavedShortcodesSearch> AddSavedShortcodesSearchAsync(SavedShortcodesSearch savedShortcodesSearch, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.GetReachabilitySourceType();

            if (sourceType == SourceType.Remote)
            {

                var criteria = savedShortcodesSearch.Criteria;
                var searchShortcodesParameters = new SearchShortcodesParameters
                {
                    Token = Token,
                    SavedSearchFilterHash = criteria.SavedSearchFilterHash,
                    MaxToFetch = criteria.MaxToFetch,
                    Name = criteria.Name,
                    Description = criteria.Description,
                    Address = criteria.Address,
                    FiledInFolderType = criteria.FiledInFolderType.ConvertEnum<DataContract.FiledInFolderType>(),
                    FiledInFolderFolderType = criteria.FiledInFolderFolderType.ConvertEnum<DataContract.FiledInFolderFolderType>()
                };

                var result = await AppServiceProxy.AddSavedSearch(new AddSavedSearchesParameters
                {
                    Token = Token,
                    ModuleType = DataContract.ModuleType.Shortcodes,
                    Name = savedShortcodesSearch.Name,
                    SavedSearchJson = Serializer.Serialize(searchShortcodesParameters)
                });

                return new SavedShortcodesSearch()
                {
                    Id = result.Id,
                    Criteria = criteria,
                    Name = savedShortcodesSearch.Name
                };

            }

            if (sourceType == SourceType.Local)
                throw new ReMarkException(ErrorConstants.Codes.InvalidSourceType);

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task SaveLastSearchDocumentsCriteriaAsync(SearchDocumentsCriteria criteria)
        {
            await FileSystemStorage.SaveLastSearchDocumentCrtiera(criteria);
        }

        public async Task SaveLastSearchContactsCriteriaAsync(SearchContactsCriteria criteria)
        {
            await FileSystemStorage.SaveLastSearchContactsCrtiera(criteria);
        }

        public async Task SaveLastSearchShortcodesCrtieriaAsync(SearchShortcodesCriteria criteria)
        {
            await FileSystemStorage.SaveLastSearchShortcodesCrtiera(criteria);
        }

        public async Task<SearchDocumentsCriteria> GetLastSearchDocumentsCriteriaAsync()
        {
            return await FileSystemStorage.GetLastSearchDocumentCrtiera();
        }

        public async Task<SearchContactsCriteria> GetLastSearchContactsCriteriaAsync()
        {
            return await FileSystemStorage.GetLastSearchContactsCrtiera();
        }

        public async Task<SearchShortcodesCriteria> GetLastSearchShortcodesCrtieriaAsync()
        {
            return await FileSystemStorage.GetLastSearchShortcodesCrtiera();
        }
     
        public async Task<List<Model.DocumentPreview>> SearchDocumentsAsync(SearchDocumentsCriteria criteria, SourceType sourceType = SourceType.Auto)
        {
            CommonConfig.UsageAnalytics.LogEvent(new DoSearchEvent(Model.ModuleType.Documents));

            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.GetReachabilitySourceType();

            if (sourceType == SourceType.Remote)
            {
                await FileSystemStorage.SaveLastSearchDocumentCrtiera(criteria);

                var result = await AppServiceProxy.SearchDocumentsAsync(new DataContract.SearchDocumentsParameters
                {
                    Token = Token,
                    SavedSearchFilterHash = criteria.SavedSearchFilterHash,
                    MaxToFetch = criteria.MaxToFetch,
                    SubjectMessageField = criteria.SubjectMessageField.SanitizeForSearch(),
                    SubjectMessageClause = criteria.SubjectMessageClause.ConvertEnum<DataContract.SubjectMessageClause>(),
                    FromToField = criteria.FromToField.SanitizeForSearch(),
                    FromToClause = criteria.FromToClause.ConvertEnum<DataContract.FromToClause>(),
                    SearchInAttachments = criteria.SearchInAttachments,
                    Unread = criteria.UnreadOnly,
                    PartialWordSearch = criteria.PartialWordSearch,
                    Processed = criteria.Handled,
                    Reference = criteria.Reference,
                    Priorities = criteria.Priorities.Select(p => p.ConvertEnum<DataContract.Priority>()).ToList(),
                    Directions = criteria.Directions.Select(p => p.ConvertEnum<DataContract.DocumentDirection>()).ToList(),
                    CategoryIds = criteria.CategoryIds.ToList(),
                    MustHaveCategoryIds = criteria.MustHaveCategoryIds.ToList(),
                    LineGuids = criteria.LineGuids.ToList(),
                    CreatorGuids = criteria.CreatorGuids.ToList(),
                    DateRange =
                    {
                        Enabled = criteria.DateRange?.Enabled ?? false,
                        Start = criteria.DateRange?.StartTimestamp.ConvertTimestampMillisecondsToDateTime() ?? default(DateTime),
                        End = criteria.DateRange?.EndTimestamp.ConvertTimestampMillisecondsToDateTime() ?? default(DateTime)
                    },
                    Comment = criteria.Comment,
                    AttachmentName = criteria.AttachmentName,
                    HavingAttachmentsOnly = criteria.HavingAttachmentsOnly,
                    FiledInFolderType = criteria.FiledInFolderType.ConvertEnum<DataContract.FiledInFolderType>(),
                    FiledInFolderFolderType = criteria.FiledInFolderFolderType.ConvertEnum<DataContract.FiledInFolderFolderType>(),
                    ExtraFields = criteria.ExtraFields
                });

                var documentPreviews = result.SearchResults.WhereNotNull().OrderByDescending(dp => dp.DateReceived).Select(dp => dp.Convert()).ToList();

                await documentsDataAccess.SaveDocumentPreviewsAsync(documentPreviews);

                return documentPreviews;
            }

            if (sourceType == SourceType.Local)
                throw new ReMarkException(ErrorConstants.Codes.InvalidSourceType);

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task<List<Model.ContactPreview>> SearchContactsAsync(SearchContactsCriteria criteria, SourceType sourceType = SourceType.Auto)
        {
            CommonConfig.UsageAnalytics.LogEvent(new DoSearchEvent(Model.ModuleType.Contacts));

            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.GetReachabilitySourceType();

            if (sourceType == SourceType.Remote)
            {
                await FileSystemStorage.SaveLastSearchContactsCrtiera(criteria);

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
                    FiledInFolderFolderType = criteria.FiledInFolderFolderType.ConvertEnum<DataContract.FiledInFolderFolderType>()
                });

                var contactPreviews = result.SearchResults.WhereNotNull().OrderBy(cp => cp.RowId).Select(cp => cp.Convert()).ToList();

                await contactsDataAccess.SaveContactPreviewsAsync(contactPreviews);

                return contactPreviews;
            }

            if (sourceType == SourceType.Local)
                throw new ReMarkException(ErrorConstants.Codes.InvalidSourceType);

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task<List<Model.ShortcodePreview>> SearchShortcodesAsync(SearchShortcodesCriteria criteria, SourceType sourceType = SourceType.Auto)
        {
            CommonConfig.UsageAnalytics.LogEvent(new DoSearchEvent(Model.ModuleType.Shortcodes));

            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.GetReachabilitySourceType();

            if (sourceType == SourceType.Remote)
            {
                await FileSystemStorage.SaveLastSearchShortcodesCrtiera(criteria);

                var result = await AppServiceProxy.SearchShortcodesAsync(new DataContract.SearchShortcodesParameters
                {
                    Token = Token,
                    SavedSearchFilterHash = criteria.SavedSearchFilterHash,
                    MaxToFetch = criteria.MaxToFetch,
                    Name = criteria.Name,
                    Description = criteria.Description,
                    Address = criteria.Address,
                    FiledInFolderType = criteria.FiledInFolderType.ConvertEnum<DataContract.FiledInFolderType>(),
                    FiledInFolderFolderType = criteria.FiledInFolderFolderType.ConvertEnum<DataContract.FiledInFolderFolderType>()
                });

                var shortcodePreviews = result.ShortcodePreviews.WhereNotNull().OrderBy(sp => sp.RowId).Select(sp => sp.Convert()).ToList();

                await shortcodesDataAccess.SaveShortcodePreviewsAsync(shortcodePreviews);

                return shortcodePreviews;
            }

            if (sourceType == SourceType.Local)
                throw new ReMarkException(ErrorConstants.Codes.InvalidSourceType);

            throw new ArgumentException("Invalid sourceType provided.");
        }

    }
}