//
// Project: Mark5.ServiceReference
// File: AppServiceClient.AppServiceClientChannel.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.ServiceModel;
using Mark5.ServiceReference.DataContract;

namespace Mark5.ServiceReference.AppService
{

    partial class AppServiceClient : ClientBase<IAppServiceClient>, IAppServiceClient
    {

        class AppServiceClientChannel : ChannelBase<IAppServiceClient>, IAppServiceClient
        {

            public AppServiceClientChannel(ClientBase<IAppServiceClient> client) : base(client)
            {
            }

            #region Authentication

            public IAsyncResult BeginAuthenticate(AuthenticationParameters parameters, AsyncCallback callback, object asyncState)
            {
                return BeginInvoke("Authenticate", new object[] { parameters }, callback, asyncState);
            }

            public AuthenticationResult EndAuthenticate(IAsyncResult asyncResult)
            {
                return (AuthenticationResult)EndInvoke("Authenticate", new object[0], asyncResult);
            }

            #endregion

            #region Folders

            public IAsyncResult BeginGetFolders(GetFoldersParameters parameters, AsyncCallback callback, object asyncState)
            {
                return BeginInvoke("GetFolders", new object[] { parameters }, callback, asyncState);
            }

            public GetFoldersResult EndGetFolders(IAsyncResult asyncResult)
            {
                return (GetFoldersResult)EndInvoke("GetFolders", new object[0], asyncResult);
            }

            #endregion

            #region Documents module

            public IAsyncResult BeginGetDocumentPreviews(GetDocumentPreviewsParameters parameters, AsyncCallback callback, object asyncState)
            {
                return BeginInvoke("GetDocumentPreviews", new object[] { parameters }, callback, asyncState);
            }

            public GetDocumentPreviewsResult EndGetDocumentPreviews(IAsyncResult asyncResult)
            {
                return (GetDocumentPreviewsResult)EndInvoke("GetDocumentPreviews", new object[0], asyncResult);
            }

            public IAsyncResult BeginGetDocument(GetDocumentParameters parameters, AsyncCallback callback, object asyncState)
            {
                return BeginInvoke("GetDocument", new object[] { parameters }, callback, asyncState);
            }

            public GetDocumentResult EndGetDocument(IAsyncResult asyncResult)
            {
                return (GetDocumentResult)EndInvoke("GetDocument", new object[0], asyncResult);
            }

            public IAsyncResult BeginSendDocument(SendDocumentParameters parameters, AsyncCallback callback, object asyncState)
            {
                return BeginInvoke("SendDocument", new object[] { parameters }, callback, asyncState);
            }

            public SendDocumentResult EndSendDocument(IAsyncResult asyncResult)
            {
                return (SendDocumentResult)EndInvoke("SendDocument", new object[0], asyncResult);
            }

            public IAsyncResult BeginSetDocumentsReadStatus(SetDocumentsReadStatusParameters parameters, AsyncCallback callback, object asyncState)
            {
                return BeginInvoke("SetDocumentsReadStatus", new object[] { parameters }, callback, asyncState);
            }

            public SetDocumentsReadStatusResult EndSetDocumentsReadStatus(IAsyncResult asyncResult)
            {
                return (SetDocumentsReadStatusResult)EndInvoke("SetDocumentsReadStatus", new object[0], asyncResult);
            }

            public IAsyncResult BeginSetDocumentPriority(SetDocumentPriorityParameters parameters, AsyncCallback callback, object asyncState)
            {
                return BeginInvoke("SetDocumentPriority", new object[] { parameters }, callback, asyncState);
            }

            public SetDocumentPriorityResult EndSetDocumentPriority(IAsyncResult asyncResult)
            {
                return (SetDocumentPriorityResult)EndInvoke("SetDocumentPriority", new object[0], asyncResult);
            }

            public IAsyncResult BeginMoveToSpam(MoveToSpamParameters parameters, AsyncCallback callback, object asyncState)
            {
                return BeginInvoke("MoveToSpam", new object[] { parameters }, callback, asyncState);
            }

            public MoveToSpamResult EndMoveToSpam(IAsyncResult asyncResult)
            {
                return (MoveToSpamResult)EndInvoke("MoveToSpam", new object[0], asyncResult);
            }

            public IAsyncResult BeginGetTemplatePreviews(GetTemplatePreviewsParameters parameters, AsyncCallback callback, object asyncState)
            {
                return BeginInvoke("GetTemplatePreviews", new object[] { parameters }, callback, asyncState);
            }

            public GetTemplatePreviewsResult EndGetTemplatePreviews(IAsyncResult asyncResult)
            {
                return (GetTemplatePreviewsResult)EndInvoke("GetTemplatePreviews", new object[0], asyncResult);
            }

            public IAsyncResult BeginGetTemplate(GetTemplateParameters parameters, AsyncCallback callback, object asyncState)
            {
                return BeginInvoke("GetTemplate", new object[] { parameters }, callback, asyncState);
            }

            public GetTemplateResult EndGetTemplate(IAsyncResult asyncResult)
            {
                return (GetTemplateResult)EndInvoke("GetTemplate", new object[0], asyncResult);
            }

            public IAsyncResult BeginGetDefaultTemplate(GetDefaultTemplateParameters parameters, AsyncCallback callback, object asyncState)
            {
                return BeginInvoke("GetDefaultTemplate", new object[] { parameters }, callback, asyncState);
            }

            public GetDefaultTemplateResult EndGetDefaultTemplate(IAsyncResult asyncResult)
            {
                return (GetDefaultTemplateResult)EndInvoke("GetDefaultTemplate", new object[0], asyncResult);
            }

            #endregion

            #region Contacts

            public IAsyncResult BeginGetContactPreviews(GetContactPreviewsParameters parameters, AsyncCallback callback, object asyncState)
            {
                return BeginInvoke("GetContactPreviews", new object[] { parameters }, callback, asyncState);
            }

            public GetContactPreviewsResult EndGetContactPreviews(IAsyncResult asyncResult)
            {
                return (GetContactPreviewsResult)EndInvoke("GetContactPreviews", new object[0], asyncResult);
            }

            public IAsyncResult BeginGetContact(GetContactParameters parameters, AsyncCallback callback, object asyncState)
            {
                return BeginInvoke("GetContact", new object[] { parameters }, callback, asyncState);
            }

            public GetContactResult EndGetContact(IAsyncResult asyncResult)
            {
                return (GetContactResult)EndInvoke("GetContact", new object[0], asyncResult);
            }

            public IAsyncResult BeginCreateOrUpdateContact(CreateOrUpdateContactParameters parameters, AsyncCallback callback, object asyncState)
            {
                return BeginInvoke("CreateOrUpdateContact", new object[] { parameters }, callback, asyncState);
            }

            public CreateOrUpdateContactResult EndCreateOrUpdateContact(IAsyncResult asyncResult)
            {
                return (CreateOrUpdateContactResult)EndInvoke("CreateOrUpdateContact", new object[0], asyncResult);
            }

            #endregion

            #region Shortcodes

            public IAsyncResult BeginGetShortcodePreviews(GetShortcodePreviewsParameters parameters, AsyncCallback callback, object asyncState)
            {
                return BeginInvoke("GetShortcodePreviews", new object[] { parameters }, callback, asyncState);
            }

            public GetShortcodePreviewsResult EndGetShortcodePreviews(IAsyncResult asyncResult)
            {
                return (GetShortcodePreviewsResult)EndInvoke("GetShortcodePreviews", new object[0], asyncResult);
            }

            public IAsyncResult BeginGetShortcode(GetShortcodeParameters parameters, AsyncCallback callback, object asyncState)
            {
                return BeginInvoke("GetShortcode", new object[] { parameters }, callback, asyncState);
            }

            public GetShortcodeResult EndGetShortcode(IAsyncResult asyncResult)
            {
                return (GetShortcodeResult)EndInvoke("GetShortcode", new object[0], asyncResult);
            }

            #endregion

            #region Calendar

            public IAsyncResult BeginGetCalendarEvents(GetCalendarEventsParameters parameters, AsyncCallback callback, object asyncState)
            {
                return BeginInvoke("GetCalendarEvents", new object[] { parameters }, callback, asyncState);
            }

            public GetCalendarEventsResult EndGetCalendarEvents(IAsyncResult asyncResult)
            {
                return (GetCalendarEventsResult)EndInvoke("GetCalendarEvents", new object[0], asyncResult);
            }

            public IAsyncResult BeginGetCalendarAppointment(GetCalendarAppointmentParameters parameters, AsyncCallback callback, object asyncState)
            {
                return BeginInvoke("GetCalendarAppointment", new object[] { parameters }, callback, asyncState);
            }

            public GetCalendarAppointmentResult EndGetCalendarAppointment(IAsyncResult asyncResult)
            {
                return (GetCalendarAppointmentResult)EndInvoke("GetCalendarAppointment", new object[0], asyncResult);
            }

            public IAsyncResult BeginGetCalendarTask(GetCalendarTaskParameters parameters, AsyncCallback callback, object asyncState)
            {
                return BeginInvoke("GetCalendarTask", new object[] { parameters }, callback, asyncState);
            }

            public GetCalendarTaskResult EndGetCalendarTask(IAsyncResult asyncResult)
            {
                return (GetCalendarTaskResult)EndInvoke("GetCalendarTask", new object[0], asyncResult);
            }

            public IAsyncResult BeginCreateOrUpdateCalendarAppointment(CreateOrUpdateCalendarAppointmentParameters parameters, AsyncCallback callback, object asyncState)
            {
                return BeginInvoke("CreateOrUpdateCalendarAppointment", new object[] { parameters }, callback, asyncState);
            }

            public CreateOrUpdateCalendarAppointmentResult EndCreateOrUpdateCalendarAppointment(IAsyncResult asyncResult)
            {
                return (CreateOrUpdateCalendarAppointmentResult)EndInvoke("CreateOrUpdateCalendarAppointment", new object[0], asyncResult);
            }

            public IAsyncResult BeginCreateOrUpdateCalendarTask(CreateOrUpdateCalendarTaskParameters parameters, AsyncCallback callback, object asyncState)
            {
                return BeginInvoke("CreateOrUpdateCalendarTask", new object[] { parameters }, callback, asyncState);
            }

            public CreateOrUpdateCalendarTaskResult EndCreateOrUpdateCalendarTask(IAsyncResult asyncResult)
            {
                return (CreateOrUpdateCalendarTaskResult)EndInvoke("CreateOrUpdateCalendarTask", new object[0], asyncResult);
            }

            #endregion

            #region Search

            public IAsyncResult BeginGetSavedSearches(GetSavedSearchesParameters parameters, AsyncCallback callback, object asyncState)
            {
                return BeginInvoke("GetSavedSearches", new object[] { parameters }, callback, asyncState);
            }

            public GetSavedSearchesResult EndGetSavedSearches(IAsyncResult asyncResult)
            {
                return (GetSavedSearchesResult)EndInvoke("GetSavedSearches", new object[0], asyncResult);
            }

            public IAsyncResult BeginSearchDocuments(SearchDocumentsParameters parameters, AsyncCallback callback, object asyncState)
            {
                return BeginInvoke("SearchDocuments", new object[] { parameters }, callback, asyncState);
            }

            public SearchDocumentsResult EndSearchDocuments(IAsyncResult asyncResult)
            {
                return (SearchDocumentsResult)EndInvoke("SearchDocuments", new object[0], asyncResult);
            }

            public IAsyncResult BeginSearchContacts(SearchContactsParameters parameters, AsyncCallback callback, object asyncState)
            {
                return BeginInvoke("SearchContacts", new object[] { parameters }, callback, asyncState);
            }

            public SearchContactsResult EndSearchContacts(IAsyncResult asyncResult)
            {
                return (SearchContactsResult)EndInvoke("SearchContacts", new object[0], asyncResult);
            }

            public IAsyncResult BeginSearchShortcodes(SearchShortcodesParameters parameters, AsyncCallback callback, object asyncState)
            {
                return BeginInvoke("SearchShortcodes", new object[] { parameters }, callback, asyncState);
            }

            public SearchShortcodesResult EndSearchShortcodes(IAsyncResult asyncResult)
            {
                return (SearchShortcodesResult)EndInvoke("SearchShortcodes", new object[0], asyncResult);
            }

            public IAsyncResult BeginSearchCalendarEvents(SearchCalendarEventsParameters parameters, AsyncCallback callback, object asyncState)
            {
                return BeginInvoke("SearchCalendarEvents", new object[] { parameters }, callback, asyncState);
            }

            public SearchCalendarEventsResult EndSearchCalendarEvents(IAsyncResult asyncResult)
            {
                return (SearchCalendarEventsResult)EndInvoke("SearchCalendarEvents", new object[0], asyncResult);
            }

            #endregion

            #region Notifications

            public IAsyncResult BeginGetNotifications(GetNotificationsParameters parameters, AsyncCallback callback, object asyncState)
            {
                return BeginInvoke("GetNotifications", new object[] { parameters }, callback, asyncState);
            }

            public GetNotificationsResult EndGetNotifications(IAsyncResult asyncResult)
            {
                return (GetNotificationsResult)EndInvoke("GetNotifications", new object[0], asyncResult);
            }

            public IAsyncResult BeginSetFoldersNotifications(SetFoldersNotificationsParameters parameters, AsyncCallback callback, object asyncState)
            {
                return BeginInvoke("SetFoldersNotifications", new object[] { parameters }, callback, asyncState);
            }

            public SetFoldersNotificationsResult EndSetFoldersNotifications(IAsyncResult asyncResult)
            {
                return (SetFoldersNotificationsResult)EndInvoke("SetFoldersNotifications", new object[0], asyncResult);
            }

            public IAsyncResult BeginGetFoldersNotifications(GetFoldersNotificationsParameters parameters, AsyncCallback callback, object asyncState)
            {
                return BeginInvoke("GetFoldersNotifications", new object[] { parameters }, callback, asyncState);
            }

            public GetFoldersNotificationsResult EndGetFoldersNotifications(IAsyncResult asyncResult)
            {
                return (GetFoldersNotificationsResult)EndInvoke("GetFoldersNotifications", new object[0], asyncResult);
            }

            public IAsyncResult BeginGetCalendarNotificationsEnabled(GetCalendarNotificationsEnabledParameters parameters, AsyncCallback callback, object asyncState)
            {
                return BeginInvoke("GetCalendarNotificationsEnabled", new object[] { parameters }, callback, asyncState);
            }

            public GetCalendarNotificationsEnabledResult EndGetCalendarNotificationsEnabled(IAsyncResult asyncResult)
            {
                return (GetCalendarNotificationsEnabledResult)EndInvoke("GetCalendarNotificationsEnabled", new object[0], asyncResult);
            }

            public IAsyncResult BeginSetCalendarNotificationsEnabled(SetCalendarNotificationsEnabledParameters parameters, AsyncCallback callback, object asyncState)
            {
                return BeginInvoke("SetCalendarNotificationsEnabled", new object[] { parameters }, callback, asyncState);
            }

            public SetCalendarNotificationsEnabledResult EndSetCalendarNotificationsEnabled(IAsyncResult asyncResult)
            {
                return (SetCalendarNotificationsEnabledResult)EndInvoke("SetCalendarNotificationsEnabled", new object[0], asyncResult);
            }

            public IAsyncResult BeginGetNotificationsSound(GetNotificationsSoundParameters parameters, AsyncCallback callback, object asyncState)
            {
                return BeginInvoke("GetNotificationsSound", new object[] { parameters }, callback, asyncState);
            }

            public GetNotificationsSoundResult EndGetNotificationsSound(IAsyncResult asyncResult)
            {
                return (GetNotificationsSoundResult)EndInvoke("GetNotificationsSound", new object[0], asyncResult);
            }

            public IAsyncResult BeginSetNotificationsSound(SetNotificationsSoundParameters parameters, AsyncCallback callback, object asyncState)
            {
                return BeginInvoke("SetNotificationsSound", new object[] { parameters }, callback, asyncState);
            }

            public SetNotificationsSoundResult EndSetNotificationsSound(IAsyncResult asyncResult)
            {
                return (SetNotificationsSoundResult)EndInvoke("SetNotificationsSound", new object[0], asyncResult);
            }

            public IAsyncResult BeginClearAllNotifications(ClearAllNotificationsParameters parameters, AsyncCallback callback, object asyncState)
            {
                return BeginInvoke("ClearAllNotifications", new object[] { parameters }, callback, asyncState);
            }

            public ClearAllNotificationsResult EndClearAllNotifications(IAsyncResult asyncResult)
            {
                return (ClearAllNotificationsResult)EndInvoke("ClearAllNotifications", new object[0], asyncResult);
            }

            #endregion

            #region Common

            public IAsyncResult BeginAddComment(AddCommentParameters parameters, AsyncCallback callback, object asyncState)
            {
                return BeginInvoke("AddComment", new object[] { parameters }, callback, asyncState);
            }

            public AddCommentResult EndAddComment(IAsyncResult asyncResult)
            {
                return (AddCommentResult)EndInvoke("AddComment", new object[0], asyncResult);
            }

            public IAsyncResult BeginEditComment(EditCommentParameters parameters, AsyncCallback callback, object asyncState)
            {
                return BeginInvoke("EditComment", new object[] { parameters }, callback, asyncState);
            }

            public EditCommentResult EndEditComment(IAsyncResult asyncResult)
            {
                return (EditCommentResult)EndInvoke("EditComment", new object[0], asyncResult);
            }

            public IAsyncResult BeginDeleteComment(DeleteCommentParameters parameters, AsyncCallback callback, object asyncState)
            {
                return BeginInvoke("DeleteComment", new object[] { parameters }, callback, asyncState);
            }

            public DeleteCommentResult EndDeleteComment(IAsyncResult asyncResult)
            {
                return (DeleteCommentResult)EndInvoke("DeleteComment", new object[0], asyncResult);
            }

            public IAsyncResult BeginGetAllCategories(GetAllCategoriesParameters parameters, AsyncCallback callback, object asyncState)
            {
                return BeginInvoke("GetAllCategories", new object[] { parameters }, callback, asyncState);
            }

            public GetAllCategoriesResult EndGetAllCategories(IAsyncResult asyncResult)
            {
                return (GetAllCategoriesResult)EndInvoke("GetAllCategories", new object[0], asyncResult);
            }

            public IAsyncResult BeginSetCategories(SetCategoriesParameters parameters, AsyncCallback callback, object asyncState)
            {
                return BeginInvoke("SetCategories", new object[] { parameters }, callback, asyncState);
            }

            public SetCategoriesResult EndSetCategories(IAsyncResult asyncResult)
            {
                return (SetCategoriesResult)EndInvoke("SetCategories", new object[0], asyncResult);
            }

            public IAsyncResult BeginGetObjectActions(GetObjectActionsParameters parameters, AsyncCallback callback, object asyncState)
            {
                return BeginInvoke("GetObjectActions", new object[] { parameters }, callback, asyncState);
            }

            public GetObjectActionsResult EndGetObjectActions(IAsyncResult asyncResult)
            {
                return (GetObjectActionsResult)EndInvoke("GetObjectActions", new object[0], asyncResult);
            }

            public IAsyncResult BeginGetObjectLinks(GetObjectLinksParameters parameters, AsyncCallback callback, object asyncState)
            {
                return BeginInvoke("GetObjectLinks", new object[] { parameters }, callback, asyncState);
            }

            public GetObjectLinksResult EndGetObjectLinks(IAsyncResult asyncResult)
            {
                return (GetObjectLinksResult)EndInvoke("GetObjectLinks", new object[0], asyncResult);
            }

            public IAsyncResult BeginGetRecentAddresses(GetRecentAddressesParameters parameters, AsyncCallback callback, object asyncState)
            {
                return BeginInvoke("GetRecentAddresses", new object[] { parameters }, callback, asyncState);
            }

            public GetRecentAddressesResult EndGetRecentAddresses(IAsyncResult asyncResult)
            {
                return (GetRecentAddressesResult)EndInvoke("GetRecentAddresses", new object[0], asyncResult);
            }

            public IAsyncResult BeginFileToFolder(FileToFolderParameters parameters, AsyncCallback callback, object asyncState)
            {
                return BeginInvoke("FileToFolder", new object[] { parameters }, callback, asyncState);
            }

            public FileToFolderResult EndFileToFolder(IAsyncResult asyncResult)
            {
                return (FileToFolderResult)EndInvoke("FileToFolder", new object[0], asyncResult);
            }

            public IAsyncResult BeginCopyToWorktray(CopyToWorktrayParameters parameters, AsyncCallback callback, object asyncState)
            {
                return BeginInvoke("CopyToWorktray", new object[] { parameters }, callback, asyncState);
            }

            public CopyToWorktrayResult EndCopyToWorktray(IAsyncResult asyncResult)
            {
                return (CopyToWorktrayResult)EndInvoke("CopyToWorktray", new object[0], asyncResult);
            }

            public IAsyncResult BeginDelete(DeleteParameters parameters, AsyncCallback callback, object asyncState)
            {
                return BeginInvoke("Delete", new object[] { parameters }, callback, asyncState);
            }

            public DeleteResult EndDelete(IAsyncResult asyncResult)
            {
                return (DeleteResult)EndInvoke("Delete", new object[0], asyncResult);
            }

            public IAsyncResult BeginRemoveFromFolder(RemoveFromFolderParameters parameters, AsyncCallback callback, object asyncState)
            {
                return BeginInvoke("RemoveFromFolder", new object[] { parameters }, callback, asyncState);
            }

            public RemoveFromFolderResult EndRemoveFromFolder(IAsyncResult asyncResult)
            {
                return (RemoveFromFolderResult)EndInvoke("RemoveFromFolder", new object[0], asyncResult);
            }

            #endregion

            #region System

            public IAsyncResult BeginGetSystemSettings(GetSystemSettingsParameters parameters, AsyncCallback callback, object asyncState)
            {
                return BeginInvoke("GetSystemSettings", new object[] { parameters }, callback, asyncState);
            }

            public GetSystemSettingsResult EndGetSystemSettings(IAsyncResult asyncResult)
            {
                return (GetSystemSettingsResult)EndInvoke("GetSystemSettings", new object[0], asyncResult);
            }

            public IAsyncResult BeginGetSystemUsers(GetSystemUsersParameters parameters, AsyncCallback callback, object asyncState)
            {
                return BeginInvoke("GetSystemUsers", new object[] { parameters }, callback, asyncState);
            }

            public GetSystemUsersResult EndGetSystemUsers(IAsyncResult asyncResult)
            {
                return (GetSystemUsersResult)EndInvoke("GetSystemUsers", new object[0], asyncResult);
            }

            #endregion

            #region Diagnostic methods

            public IAsyncResult BeginTest(TestParameters parameters, AsyncCallback callback, object asyncState)
            {
                return BeginInvoke("Test", new object[] { parameters }, callback, asyncState);
            }

            public TestResult EndTest(IAsyncResult asyncResult)
            {
                return (TestResult)EndInvoke("Test", new object[0], asyncResult);
            }

            #endregion

        }
    }
}

