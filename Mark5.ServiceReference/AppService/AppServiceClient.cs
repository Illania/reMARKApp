//
// File: AppServiceClient.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//

using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using Mark5.ServiceReference.DataContract;

namespace Mark5.ServiceReference.AppService
{
    partial class AppServiceClient : ClientBase<IAppServiceClient>, IAppServiceClient
    {
        public AppServiceClient(Binding binding, EndpointAddress remoteAddress)
            : base(binding, remoteAddress)
        {
        }

        protected override IAppServiceClient CreateChannel()
        {
            return new AppServiceClientChannel(this);
        }

        #region Authentication

        public IAsyncResult BeginAuthenticate(AuthenticateParameters parameters, AsyncCallback callback, object asyncState)
        {
            return Channel.BeginAuthenticate(parameters, callback, asyncState);
        }

        public AuthenticateResult EndAuthenticate(IAsyncResult asyncResult)
        {
            return Channel.EndAuthenticate(asyncResult);
        }

        #endregion

        #region Folders

        public IAsyncResult BeginGetFolders(GetFoldersParameters parameters, AsyncCallback callback, object asyncState)
        {
            return Channel.BeginGetFolders(parameters, callback, asyncState);
        }

        public GetFoldersResult EndGetFolders(IAsyncResult asyncResult)
        {
            return Channel.EndGetFolders(asyncResult);
        }

        #endregion

        #region Documents module

        public IAsyncResult BeginGetDocumentPreviews(GetDocumentPreviewsParameters parameters, AsyncCallback callback, object asyncState)
        {
            return Channel.BeginGetDocumentPreviews(parameters, callback, asyncState);
        }

        public GetDocumentPreviewsResult EndGetDocumentPreviews(IAsyncResult asyncResult)
        {
            return Channel.EndGetDocumentPreviews(asyncResult);
        }

        public IAsyncResult BeginGetDocument(GetDocumentParameters parameters, AsyncCallback callback, object asyncState)
        {
            return Channel.BeginGetDocument(parameters, callback, asyncState);
        }

        public GetDocumentResult EndGetDocument(IAsyncResult asyncResult)
        {
            return Channel.EndGetDocument(asyncResult);
        }

        public IAsyncResult BeginSendDocument(SendDocumentParameters parameters, AsyncCallback callback, object asyncState)
        {
            return Channel.BeginSendDocument(parameters, callback, asyncState);
        }

        public SendDocumentResult EndSendDocument(IAsyncResult asyncResult)
        {
            return Channel.EndSendDocument(asyncResult);
        }

        public IAsyncResult BeginSetDocumentsReadStatus(SetDocumentsReadStatusParameters parameters, AsyncCallback callback, object asyncState)
        {
            return Channel.BeginSetDocumentsReadStatus(parameters, callback, asyncState);
        }

        public SetDocumentsReadStatusResult EndSetDocumentsReadStatus(IAsyncResult asyncResult)
        {
            return Channel.EndSetDocumentsReadStatus(asyncResult);
        }

        public IAsyncResult BeginSetDocumentPriority(SetDocumentPriorityParameters parameters, AsyncCallback callback, object asyncState)
        {
            return Channel.BeginSetDocumentPriority(parameters, callback, asyncState);
        }

        public SetDocumentPriorityResult EndSetDocumentPriority(IAsyncResult asyncResult)
        {
            return Channel.EndSetDocumentPriority(asyncResult);
        }

        public IAsyncResult BeginMoveToSpam(MoveToSpamParameters parameters, AsyncCallback callback, object asyncState)
        {
            return Channel.BeginMoveToSpam(parameters, callback, asyncState);
        }

        public MoveToSpamResult EndMoveToSpam(IAsyncResult asyncResult)
        {
            return Channel.EndMoveToSpam(asyncResult);
        }

        public IAsyncResult BeginGetTemplatePreviews(GetTemplatePreviewsParameters parameters, AsyncCallback callback, object asyncState)
        {
            return Channel.BeginGetTemplatePreviews(parameters, callback, asyncState);
        }

        public GetTemplatePreviewsResult EndGetTemplatePreviews(IAsyncResult asyncResult)
        {
            return Channel.EndGetTemplatePreviews(asyncResult);
        }

        public IAsyncResult BeginGetTemplate(GetTemplateParameters parameters, AsyncCallback callback, object asyncState)
        {
            return Channel.BeginGetTemplate(parameters, callback, asyncState);
        }

        public GetTemplateResult EndGetTemplate(IAsyncResult asyncResult)
        {
            return Channel.EndGetTemplate(asyncResult);
        }

        public IAsyncResult BeginGetDefaultTemplate(GetDefaultTemplateParameters parameters, AsyncCallback callback, object asyncState)
        {
            return Channel.BeginGetDefaultTemplate(parameters, callback, asyncState);
        }

        public GetDefaultTemplateResult EndGetDefaultTemplate(IAsyncResult asyncResult)
        {
            return Channel.EndGetDefaultTemplate(asyncResult);
        }

        #endregion

        #region Contacts

        public IAsyncResult BeginGetContactPreviews(GetContactPreviewsParameters parameters, AsyncCallback callback, object asyncState)
        {
            return Channel.BeginGetContactPreviews(parameters, callback, asyncState);
        }

        public GetContactPreviewsResult EndGetContactPreviews(IAsyncResult asyncResult)
        {
            return Channel.EndGetContactPreviews(asyncResult);
        }

        public IAsyncResult BeginGetContact(GetContactParameters parameters, AsyncCallback callback, object asyncState)
        {
            return Channel.BeginGetContact(parameters, callback, asyncState);
        }

        public GetContactResult EndGetContact(IAsyncResult asyncResult)
        {
            return Channel.EndGetContact(asyncResult);
        }

        public IAsyncResult BeginCreateOrUpdateContact(CreateOrUpdateContactParameters parameters, AsyncCallback callback, object asyncState)
        {
            return Channel.BeginCreateOrUpdateContact(parameters, callback, asyncState);
        }

        public CreateOrUpdateContactResult EndCreateOrUpdateContact(IAsyncResult asyncResult)
        {
            return Channel.EndCreateOrUpdateContact(asyncResult);
        }

        #endregion

        #region Shortcodes

        public IAsyncResult BeginGetShortcodePreviews(GetShortcodePreviewsParameters parameters, AsyncCallback callback, object asyncState)
        {
            return Channel.BeginGetShortcodePreviews(parameters, callback, asyncState);
        }

        public GetShortcodePreviewsResult EndGetShortcodePreviews(IAsyncResult asyncResult)
        {
            return Channel.EndGetShortcodePreviews(asyncResult);
        }

        public IAsyncResult BeginGetShortcode(GetShortcodeParameters parameters, AsyncCallback callback, object asyncState)
        {
            return Channel.BeginGetShortcode(parameters, callback, asyncState);
        }

        public GetShortcodeResult EndGetShortcode(IAsyncResult asyncResult)
        {
            return Channel.EndGetShortcode(asyncResult);
        }

        #endregion

        #region Calendar

        public IAsyncResult BeginGetCalendarEvents(GetCalendarEventsParameters parameters, AsyncCallback callback, object asyncState)
        {
            return Channel.BeginGetCalendarEvents(parameters, callback, asyncState);
        }

        public GetCalendarEventsResult EndGetCalendarEvents(IAsyncResult asyncResult)
        {
            return Channel.EndGetCalendarEvents(asyncResult);
        }

        public IAsyncResult BeginGetCalendarAppointment(GetCalendarAppointmentParameters parameters, AsyncCallback callback, object asyncState)
        {
            return Channel.BeginGetCalendarAppointment(parameters, callback, asyncState);
        }

        public GetCalendarAppointmentResult EndGetCalendarAppointment(IAsyncResult asyncResult)
        {
            return Channel.EndGetCalendarAppointment(asyncResult);
        }

        public IAsyncResult BeginGetCalendarTask(GetCalendarTaskParameters parameters, AsyncCallback callback, object asyncState)
        {
            return Channel.BeginGetCalendarTask(parameters, callback, asyncState);
        }

        public GetCalendarTaskResult EndGetCalendarTask(IAsyncResult asyncResult)
        {
            return Channel.EndGetCalendarTask(asyncResult);
        }

        public IAsyncResult BeginCreateOrUpdateCalendarAppointment(CreateOrUpdateCalendarAppointmentParameters parameters, AsyncCallback callback, object asyncState)
        {
            return Channel.BeginCreateOrUpdateCalendarAppointment(parameters, callback, asyncState);
        }

        public CreateOrUpdateCalendarAppointmentResult EndCreateOrUpdateCalendarAppointment(IAsyncResult asyncResult)
        {
            return Channel.EndCreateOrUpdateCalendarAppointment(asyncResult);
        }

        public IAsyncResult BeginCreateOrUpdateCalendarTask(CreateOrUpdateCalendarTaskParameters parameters, AsyncCallback callback, object asyncState)
        {
            return Channel.BeginCreateOrUpdateCalendarTask(parameters, callback, asyncState);
        }

        public CreateOrUpdateCalendarTaskResult EndCreateOrUpdateCalendarTask(IAsyncResult asyncResult)
        {
            return Channel.EndCreateOrUpdateCalendarTask(asyncResult);
        }

        #endregion

        #region Search

        public IAsyncResult BeginGetSavedSearches(GetSavedSearchesParameters parameters, AsyncCallback callback, object asyncState)
        {
            return Channel.BeginGetSavedSearches(parameters, callback, asyncState);
        }

        public GetSavedSearchesResult EndGetSavedSearches(IAsyncResult asyncResult)
        {
            return Channel.EndGetSavedSearches(asyncResult);
        }

        public IAsyncResult BeginSearchDocuments(SearchDocumentsParameters parameters, AsyncCallback callback, object asyncState)
        {
            return Channel.BeginSearchDocuments(parameters, callback, asyncState);
        }

        public SearchDocumentsResult EndSearchDocuments(IAsyncResult asyncResult)
        {
            return Channel.EndSearchDocuments(asyncResult);
        }

        public IAsyncResult BeginSearchContacts(SearchContactsParameters parameters, AsyncCallback callback, object asyncState)
        {
            return Channel.BeginSearchContacts(parameters, callback, asyncState);
        }

        public SearchContactsResult EndSearchContacts(IAsyncResult asyncResult)
        {
            return Channel.EndSearchContacts(asyncResult);
        }

        public IAsyncResult BeginSearchShortcodes(SearchShortcodesParameters parameters, AsyncCallback callback, object asyncState)
        {
            return Channel.BeginSearchShortcodes(parameters, callback, asyncState);
        }

        public SearchShortcodesResult EndSearchShortcodes(IAsyncResult asyncResult)
        {
            return Channel.EndSearchShortcodes(asyncResult);
        }

        public IAsyncResult BeginSearchCalendarEvents(SearchCalendarEventsParameters parameters, AsyncCallback callback, object asyncState)
        {
            return Channel.BeginSearchCalendarEvents(parameters, callback, asyncState);
        }

        public SearchCalendarEventsResult EndSearchCalendarEvents(IAsyncResult asyncResult)
        {
            return Channel.EndSearchCalendarEvents(asyncResult);
        }

        #endregion

        #region Notifications

        public IAsyncResult BeginGetNotifications(GetNotificationsParameters parameters, AsyncCallback callback, object asyncState)
        {
            return Channel.BeginGetNotifications(parameters, callback, asyncState);
        }

        public GetNotificationsResult EndGetNotifications(IAsyncResult asyncResult)
        {
            return Channel.EndGetNotifications(asyncResult);
        }

        public IAsyncResult BeginSetFoldersNotifications(SetFoldersNotificationsParameters parameters, AsyncCallback callback, object asyncState)
        {
            return Channel.BeginSetFoldersNotifications(parameters, callback, asyncState);
        }

        public SetFoldersNotificationsResult EndSetFoldersNotifications(IAsyncResult asyncResult)
        {
            return Channel.EndSetFoldersNotifications(asyncResult);
        }

        public IAsyncResult BeginGetFoldersNotifications(GetFoldersNotificationsParameters parameters, AsyncCallback callback, object asyncState)
        {
            return Channel.BeginGetFoldersNotifications(parameters, callback, asyncState);
        }

        public GetFoldersNotificationsResult EndGetFoldersNotifications(IAsyncResult asyncResult)
        {
            return Channel.EndGetFoldersNotifications(asyncResult);
        }

        public IAsyncResult BeginGetCalendarNotificationsEnabled(GetCalendarNotificationsEnabledParameters parameters, AsyncCallback callback, object asyncState)
        {
            return Channel.BeginGetCalendarNotificationsEnabled(parameters, callback, asyncState);
        }

        public GetCalendarNotificationsEnabledResult EndGetCalendarNotificationsEnabled(IAsyncResult asyncResult)
        {
            return Channel.EndGetCalendarNotificationsEnabled(asyncResult);
        }

        public IAsyncResult BeginSetCalendarNotificationsEnabled(SetCalendarNotificationsEnabledParameters parameters, AsyncCallback callback, object asyncState)
        {
            return Channel.BeginSetCalendarNotificationsEnabled(parameters, callback, asyncState);
        }

        public SetCalendarNotificationsEnabledResult EndSetCalendarNotificationsEnabled(IAsyncResult asyncResult)
        {
            return Channel.EndSetCalendarNotificationsEnabled(asyncResult);
        }

        public IAsyncResult BeginGetNotificationsSound(GetNotificationsSoundParameters parameters, AsyncCallback callback, object asyncState)
        {
            return Channel.BeginGetNotificationsSound(parameters, callback, asyncState);
        }

        public GetNotificationsSoundResult EndGetNotificationsSound(IAsyncResult asyncResult)
        {
            return Channel.EndGetNotificationsSound(asyncResult);
        }

        public IAsyncResult BeginSetNotificationsSound(SetNotificationsSoundParameters parameters, AsyncCallback callback, object asyncState)
        {
            return Channel.BeginSetNotificationsSound(parameters, callback, asyncState);
        }

        public SetNotificationsSoundResult EndSetNotificationsSound(IAsyncResult asyncResult)
        {
            return Channel.EndSetNotificationsSound(asyncResult);
        }

        public IAsyncResult BeginClearAllNotifications(ClearAllNotificationsParameters parameters, AsyncCallback callback, object asyncState)
        {
            return Channel.BeginClearAllNotifications(parameters, callback, asyncState);
        }

        public ClearAllNotificationsResult EndClearAllNotifications(IAsyncResult asyncResult)
        {
            return Channel.EndClearAllNotifications(asyncResult);
        }

        #endregion

        #region Common

        public IAsyncResult BeginAddComment(AddCommentParameters parameters, AsyncCallback callback, object asyncState)
        {
            return Channel.BeginAddComment(parameters, callback, asyncState);
        }

        public AddCommentResult EndAddComment(IAsyncResult asyncResult)
        {
            return Channel.EndAddComment(asyncResult);
        }

        public IAsyncResult BeginEditComment(EditCommentParameters parameters, AsyncCallback callback, object asyncState)
        {
            return Channel.BeginEditComment(parameters, callback, asyncState);
        }

        public EditCommentResult EndEditComment(IAsyncResult asyncResult)
        {
            return Channel.EndEditComment(asyncResult);
        }

        public IAsyncResult BeginDeleteComment(DeleteCommentParameters parameters, AsyncCallback callback, object asyncState)
        {
            return Channel.BeginDeleteComment(parameters, callback, asyncState);
        }

        public DeleteCommentResult EndDeleteComment(IAsyncResult asyncResult)
        {
            return Channel.EndDeleteComment(asyncResult);
        }

        public IAsyncResult BeginGetAllCategories(GetAllCategoriesParameters parameters, AsyncCallback callback, object asyncState)
        {
            return Channel.BeginGetAllCategories(parameters, callback, asyncState);
        }

        public GetAllCategoriesResult EndGetAllCategories(IAsyncResult asyncResult)
        {
            return Channel.EndGetAllCategories(asyncResult);
        }

        public IAsyncResult BeginSetCategories(SetCategoriesParameters parameters, AsyncCallback callback, object asyncState)
        {
            return Channel.BeginSetCategories(parameters, callback, asyncState);
        }

        public SetCategoriesResult EndSetCategories(IAsyncResult asyncResult)
        {
            return Channel.EndSetCategories(asyncResult);
        }

        public IAsyncResult BeginGetObjectActions(GetObjectActionsParameters parameters, AsyncCallback callback, object asyncState)
        {
            return Channel.BeginGetObjectActions(parameters, callback, asyncState);
        }

        public GetObjectActionsResult EndGetObjectActions(IAsyncResult asyncResult)
        {
            return Channel.EndGetObjectActions(asyncResult);
        }

        public IAsyncResult BeginGetObjectLinks(GetObjectLinksParameters parameters, AsyncCallback callback, object asyncState)
        {
            return Channel.BeginGetObjectLinks(parameters, callback, asyncState);
        }

        public GetObjectLinksResult EndGetObjectLinks(IAsyncResult asyncResult)
        {
            return Channel.EndGetObjectLinks(asyncResult);
        }

        public IAsyncResult BeginGetRecentAddresses(GetRecentAddressesParameters parameters, AsyncCallback callback, object asyncState)
        {
            return Channel.BeginGetRecentAddresses(parameters, callback, asyncState);
        }

        public GetRecentAddressesResult EndGetRecentAddresses(IAsyncResult asyncResult)
        {
            return Channel.EndGetRecentAddresses(asyncResult);
        }

        public IAsyncResult BeginFileToFolder(FileToFolderParameters parameters, AsyncCallback callback, object asyncState)
        {
            return Channel.BeginFileToFolder(parameters, callback, asyncState);
        }

        public FileToFolderResult EndFileToFolder(IAsyncResult asyncResult)
        {
            return Channel.EndFileToFolder(asyncResult);
        }

        public IAsyncResult BeginCopyToWorktray(CopyToWorktrayParameters parameters, AsyncCallback callback, object asyncState)
        {
            return Channel.BeginCopyToWorktray(parameters, callback, asyncState);
        }

        public CopyToWorktrayResult EndCopyToWorktray(IAsyncResult asyncResult)
        {
            return Channel.EndCopyToWorktray(asyncResult);
        }

        public IAsyncResult BeginDelete(DeleteParameters parameters, AsyncCallback callback, object asyncState)
        {
            return Channel.BeginDelete(parameters, callback, asyncState);
        }

        public DeleteResult EndDelete(IAsyncResult asyncResult)
        {
            return Channel.EndDelete(asyncResult);
        }

        public IAsyncResult BeginRemoveFromFolder(RemoveFromFolderParameters parameters, AsyncCallback callback, object asyncState)
        {
            return Channel.BeginRemoveFromFolder(parameters, callback, asyncState);
        }

        public RemoveFromFolderResult EndRemoveFromFolder(IAsyncResult asyncResult)
        {
            return Channel.EndRemoveFromFolder(asyncResult);
        }

        #endregion

        #region System

        public IAsyncResult BeginGetSystemSettings(GetSystemSettingsParameters parameters, AsyncCallback callback, object asyncState)
        {
            return Channel.BeginGetSystemSettings(parameters, callback, asyncState);
        }

        public GetSystemSettingsResult EndGetSystemSettings(IAsyncResult asyncResult)
        {
            return Channel.EndGetSystemSettings(asyncResult);
        }

        public IAsyncResult BeginGetSystemUsers(GetSystemUsersParameters parameters, AsyncCallback callback, object asyncState)
        {
            return Channel.BeginGetSystemUsers(parameters, callback, asyncState);
        }

        public GetSystemUsersResult EndGetSystemUsers(IAsyncResult asyncResult)
        {
            return Channel.EndGetSystemUsers(asyncResult);
        }

        #endregion

        #region Diagnostic methods

        public IAsyncResult BeginTest(TestParameters parameters, AsyncCallback callback, object asyncState)
        {
            return Channel.BeginTest(parameters, callback, asyncState);
        }

        public TestResult EndTest(IAsyncResult asyncResult)
        {
            return Channel.EndTest(asyncResult);
        }

        #endregion
    }
}