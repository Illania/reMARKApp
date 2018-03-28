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

namespace Mark5.Mobile.Common.Manager
{
    class SearchManager : AbstractManager, ISearchManager
    {
        public DocumentBodyTypeRequest DocumentBodyTypeRequest { get; set; } = DocumentBodyTypeRequest.HtmlOnly;

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

        public async Task<List<SavedSearch>> GetSavedSearches(SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.IsReachable ? SourceType.Remote : SourceType.Local;

            if (sourceType == SourceType.Remote)
            {
                var result = await AppServiceProxy.GetSavedSearchesAsync(new DataContract.GetSavedSearchesParameters
                {
                    Token = Token
                });

                return result.SavedSearches.WhereNotNull().OrderBy(ss => ss.Name).Select(ss => ss.Convert()).ToList();
            }

            if (sourceType == SourceType.Local)
                throw new InvalidSourceTypeException("This action can only be performed when online.");

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

        public async Task<List<DocumentPreview>> SearchDocumentsAsync(SearchDocumentsCriteria criteria, SourceType sourceType = SourceType.Auto)
        {
            CommonConfig.UsageAnalytics.LogEvent(new DoSearchEvent(ModuleType.Documents));

            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.IsReachable ? SourceType.Remote : SourceType.Local;

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
                throw new InvalidSourceTypeException("This action can only be performed when online.");

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task<List<ContactPreview>> SearchContactsAsync(SearchContactsCriteria criteria, SourceType sourceType = SourceType.Auto)
        {
            CommonConfig.UsageAnalytics.LogEvent(new DoSearchEvent(ModuleType.Contacts));

            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.IsReachable ? SourceType.Remote : SourceType.Local;

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
                throw new InvalidSourceTypeException("This action can only be performed when online.");

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task<List<ShortcodePreview>> SearchShortcodesAsync(SearchShortcodesCriteria criteria, SourceType sourceType = SourceType.Auto)
        {
            CommonConfig.UsageAnalytics.LogEvent(new DoSearchEvent(ModuleType.Shortcodes));

            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.IsReachable ? SourceType.Remote : SourceType.Local;

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
                throw new InvalidSourceTypeException("This action can only be performed when online.");

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task<List<CalendarTask>> SearchCalendarTasksAsync(SearchCalendarEventsCriteria criteria, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.IsReachable ? SourceType.Remote : SourceType.Local;

            if (sourceType == SourceType.Remote)
            {
                var result = await AppServiceProxy.SearchCalendarEventsAsync(new DataContract.SearchCalendarEventsParameters
                {
                    Type = DataContract.SearchCalendarEventsType.Tasks,
                    SavedSearchFilterHash = criteria.SavedSearchFilterHash,
                    InCalendarOfUserIds = criteria.InCalendarOfUserIds,
                    Priority = criteria.Priority.ConvertEnum<DataContract.Priority>(),
                    Subject = criteria.Subject,
                    Description = criteria.Description,
                    InGroupCalendarOfUserIds = criteria.InGroupCalendarOfUserIds,
                    TaskCreatedByUserIds = criteria.TaskCreatedByUserIds,
                    DelegatedToUserIds = criteria.DelegatedToUserIds,
                    DelegatedToDepartmentIds = criteria.DelegatedToDepartmentIds,
                    CalendarCategoryIds = criteria.CalendarCategoryIds,
                    Location = criteria.Location,
                    ParticipantUserIds = criteria.ParticipantUserIds,
                    DateRange =
                    {
                        Enabled = criteria.DateRange?.Enabled ?? false,
                        Start = criteria.DateRange?.StartTimestamp.ConvertTimestampMillisecondsToDateTime() ?? default(DateTime),
                        End = criteria.DateRange?.EndTimestamp.ConvertTimestampMillisecondsToDateTime() ?? default(DateTime)
                    },
                    FiledInFolderType = criteria.FiledInFolderType.ConvertEnum<DataContract.FiledInFolderType>(),
                    FiledInFolderFolderType = criteria.FiledInFolderFolderType.ConvertEnum<DataContract.FiledInFolderFolderType>(),
                    FiledInFolderIds = criteria.FiledInFolderIds
                });

                return result.CalendarTasks.WhereNotNull().Select(ct => ct.Convert()).ToList();
            }

            if (sourceType == SourceType.Local)
                throw new InvalidSourceTypeException("This action can only be performed when online.");

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task<List<CalendarAppointment>> SearchCalendarAppointmentsAsync(SearchCalendarEventsCriteria criteria, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.IsReachable ? SourceType.Remote : SourceType.Local;

            if (sourceType == SourceType.Remote)
            {
                var result = await AppServiceProxy.SearchCalendarEventsAsync(new DataContract.SearchCalendarEventsParameters
                {
                    Type = DataContract.SearchCalendarEventsType.Appointments,
                    SavedSearchFilterHash = criteria.SavedSearchFilterHash,
                    InCalendarOfUserIds = criteria.InCalendarOfUserIds,
                    Priority = criteria.Priority.ConvertEnum<DataContract.Priority>(),
                    Subject = criteria.Subject,
                    Description = criteria.Description,
                    InGroupCalendarOfUserIds = criteria.InGroupCalendarOfUserIds,
                    TaskCreatedByUserIds = criteria.TaskCreatedByUserIds,
                    DelegatedToUserIds = criteria.DelegatedToUserIds,
                    DelegatedToDepartmentIds = criteria.DelegatedToDepartmentIds,
                    CalendarCategoryIds = criteria.CalendarCategoryIds,
                    Location = criteria.Location,
                    ParticipantUserIds = criteria.ParticipantUserIds,
                    DateRange =
                    {
                        Enabled = criteria.DateRange?.Enabled ?? false,
                        Start = criteria.DateRange?.StartTimestamp.ConvertTimestampMillisecondsToDateTime() ?? default(DateTime),
                        End = criteria.DateRange?.EndTimestamp.ConvertTimestampMillisecondsToDateTime() ?? default(DateTime)
                    },
                    FiledInFolderType = criteria.FiledInFolderType.ConvertEnum<DataContract.FiledInFolderType>(),
                    FiledInFolderFolderType = criteria.FiledInFolderFolderType.ConvertEnum<DataContract.FiledInFolderFolderType>(),
                    FiledInFolderIds = criteria.FiledInFolderIds
                });

                return result.CalendarAppointments.WhereNotNull().Select(ct => ct.Convert()).ToList();
            }

            if (sourceType == SourceType.Local)
                throw new InvalidSourceTypeException("This action can only be performed when online.");

            throw new ArgumentException("Invalid sourceType provided.");
        }
    }
}