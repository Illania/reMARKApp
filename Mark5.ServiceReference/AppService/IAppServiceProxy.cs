using System;
using System.Threading;
using System.Threading.Tasks;
using Mark5.ServiceReference.DataContract;

namespace Mark5.ServiceReference.AppService
{
    public interface IAppServiceProxy
    {
        Version Version { get; }

        #region Authentication

        Task<AuthenticateResult> AuthenticateAsync(AuthenticateParameters parameters, CancellationToken ct = default(CancellationToken));

        #endregion

        #region Folders

        Task<GetFoldersResult> GetFoldersAsync(GetFoldersParameters parameters, CancellationToken ct = default(CancellationToken));

        #endregion

        #region Documents module

        Task<GetDocumentPreviewsResult> GetDocumentPreviewsAsync(GetDocumentPreviewsParameters parameters, CancellationToken ct = default(CancellationToken));

        Task<GetDocumentResult> GetDocumentAsync(GetDocumentParameters parameters, CancellationToken ct = default(CancellationToken));

        Task<SendDocumentResult> SendDocumentAsync(SendDocumentParameters parameters, CancellationToken ct = default(CancellationToken));

        Task<SetDocumentsReadStatusResult> SetDocumentsReadStatusAsync(SetDocumentsReadStatusParameters parameters, CancellationToken ct = default(CancellationToken));

        Task<SetDocumentPriorityResult> SetDocumentPriorityAsync(SetDocumentPriorityParameters parameters, CancellationToken ct = default(CancellationToken));

        Task<MoveToSpamResult> MoveToSpamAsync(MoveToSpamParameters parameters, CancellationToken ct = default(CancellationToken));

        Task<GetTemplatePreviewsResult> GetTemplatePreviewsAsync(GetTemplatePreviewsParameters parameters, CancellationToken ct = default(CancellationToken));

        Task<GetTemplateResult> GetTemplateAsync(GetTemplateParameters parameters, CancellationToken ct = default(CancellationToken));

        Task<GetDefaultTemplateResult> GetDefaultTemplateAsync(GetDefaultTemplateParameters parameters, CancellationToken ct = default(CancellationToken));

        #endregion

        #region Contacts module

        Task<GetContactPreviewsResult> GetContactPreviewsAsync(GetContactPreviewsParameters parameters, CancellationToken ct = default(CancellationToken));

        Task<GetContactResult> GetContactAsync(GetContactParameters parameters, CancellationToken ct = default(CancellationToken));

        Task<CreateOrUpdateContactResult> CreateOrUpdateContactAsync(CreateOrUpdateContactParameters parameters, CancellationToken ct = default(CancellationToken));

        #endregion

        #region Shortcodes module

        Task<GetShortcodePreviewsResult> GetShortcodePreviewsAsync(GetShortcodePreviewsParameters parameters, CancellationToken ct = default(CancellationToken));

        Task<GetShortcodeResult> GetShortcodeAsync(GetShortcodeParameters parameters, CancellationToken ct = default(CancellationToken));

        #endregion

        #region Calendar module

        Task<GetCalendarEventsResult> GetCalendarEventsAsync(GetCalendarEventsParameters parameters, CancellationToken ct = default(CancellationToken));

        Task<GetCalendarAppointmentResult> GetCalendarAppointmentAsync(GetCalendarAppointmentParameters parameters, CancellationToken ct = default(CancellationToken));

        Task<GetCalendarTaskResult> GetCalendarTaskAsync(GetCalendarTaskParameters parameters, CancellationToken ct = default(CancellationToken));

        Task<CreateOrUpdateCalendarAppointmentResult> CreateOrUpdateCalendarAppointmentAsync(CreateOrUpdateCalendarAppointmentParameters parameters, CancellationToken ct = default(CancellationToken));

        Task<CreateOrUpdateCalendarTaskResult> CreateOrUpdateCalendarTaskAsync(CreateOrUpdateCalendarTaskParameters parameters, CancellationToken ct = default(CancellationToken));

        #endregion

        #region Search

        Task<GetSavedSearchesResult> GetSavedSearchesAsync(GetSavedSearchesParameters parameters, CancellationToken ct = default(CancellationToken));

        Task<SearchDocumentsResult> SearchDocumentsAsync(SearchDocumentsParameters parameters, CancellationToken ct = default(CancellationToken));

        Task<SearchContactsResult> SearchContactsAsync(SearchContactsParameters parameters, CancellationToken ct = default(CancellationToken));

        Task<SearchShortcodesResult> SearchShortcodesAsync(SearchShortcodesParameters parameters, CancellationToken ct = default(CancellationToken));

        Task<SearchCalendarEventsResult> SearchCalendarEventsAsync(SearchCalendarEventsParameters parameters, CancellationToken ct = default(CancellationToken));

        #endregion

        #region Notifications

        Task<GetNotificationsResult> GetNotificationsAsync(GetNotificationsParameters parameters, CancellationToken ct = default(CancellationToken));

        Task<SetFoldersNotificationsResult> SetFoldersNotificationsAsync(SetFoldersNotificationsParameters parameters, CancellationToken ct = default(CancellationToken));

        Task<GetFoldersNotificationsResult> GetFoldersNotificationsAsync(GetFoldersNotificationsParameters parameters, CancellationToken ct = default(CancellationToken));

        Task<GetCalendarNotificationsEnabledResult> GetCalendarNotificationsEnabledAsync(GetCalendarNotificationsEnabledParameters parameters, CancellationToken ct = default(CancellationToken));

        Task<SetCalendarNotificationsEnabledResult> SetCalendarNotificationsEnabledAsync(SetCalendarNotificationsEnabledParameters parameters, CancellationToken ct = default(CancellationToken));

        Task<GetNotificationsSoundResult> GetNotificationsSoundAsync(GetNotificationsSoundParameters parameters, CancellationToken ct = default(CancellationToken));

        Task<SetNotificationsSoundResult> SetNotificationsSoundAsync(SetNotificationsSoundParameters parameters, CancellationToken ct = default(CancellationToken));

        Task<ClearAllNotificationsResult> ClearAllNotificationsAsync(ClearAllNotificationsParameters parameters, CancellationToken ct = default(CancellationToken));

        #endregion

        #region Common

        Task<AddCommentResult> AddCommentAsync(AddCommentParameters parameters, CancellationToken ct = default(CancellationToken));

        Task<EditCommentResult> EditCommentAsync(EditCommentParameters parameters, CancellationToken ct = default(CancellationToken));

        Task<DeleteCommentResult> DeleteCommentAsync(DeleteCommentParameters parameters, CancellationToken ct = default(CancellationToken));

        Task<GetAllCategoriesResult> GetAllCategoriesAsync(GetAllCategoriesParameters parameters, CancellationToken ct = default(CancellationToken));

        Task<SetCategoriesResult> SetCategoriesAsync(SetCategoriesParameters parameters, CancellationToken ct = default(CancellationToken));

        Task<GetObjectActionsResult> GetObjectActionsAsync(GetObjectActionsParameters parameters, CancellationToken ct = default(CancellationToken));

        Task<GetObjectLinksResult> GetObjectLinksAsync(GetObjectLinksParameters parameters, CancellationToken ct = default(CancellationToken));

        Task<GetRecentAddressesResult> GetRecentAddressesAsync(GetRecentAddressesParameters parameters, CancellationToken ct = default(CancellationToken));

        Task<FileToFolderResult> FileToFolderAsync(FileToFolderParameters parameters, CancellationToken ct = default(CancellationToken));

        Task<CopyToWorktrayResult> CopyToWorktrayAsync(CopyToWorktrayParameters parameters, CancellationToken ct = default(CancellationToken));

        Task<DeleteResult> DeleteAsync(DeleteParameters parameters, CancellationToken ct = default(CancellationToken));

        Task<RemoveFromFolderResult> RemoveFromFolderAsync(RemoveFromFolderParameters parameters, CancellationToken ct = default(CancellationToken));

        #endregion

        #region System

        Task<GetSystemSettingsResult> GetSystemSettingsAsync(GetSystemSettingsParameters parameters, CancellationToken ct = default(CancellationToken));

        Task<GetSystemUsersResult> GetSystemUsersAsync(GetSystemUsersParameters parameters, CancellationToken ct = default(CancellationToken));

        #endregion

        #region Diagnostic methods

        Task<TestResult> TestAsync(TestParameters parameters, CancellationToken ct = default(CancellationToken));

        #endregion
    }
}