//
// File: IAppServiceClient.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//

using System;
using System.ServiceModel;
using Mark5.ServiceReference.DataContract;
using Mark5.ServiceReference.Exceptions;

namespace Mark5.ServiceReference.AppService
{
    [ServiceContract(Name = "AppService.v3", Namespace = "com.nordic-it.appservice.v3", ConfigurationName = "Mark5.Mobile.Common.ServiceContract.IAppServicev3")]
    interface IAppServiceClient
    {
        #region Authentication

        [OperationContract(Action = "Authenticate", AsyncPattern = true)]
        [FaultContract(typeof(AppServiceFaultDetail), Name = "AppServiceFaultDetail", Namespace = "com.nordic-it.appservice.v3")]
        IAsyncResult BeginAuthenticate(AuthenticateParameters parameters, AsyncCallback callback, object asyncState);

        AuthenticateResult EndAuthenticate(IAsyncResult asyncResult);

        #endregion

        #region Folders

        [OperationContract(Action = "GetFolders", AsyncPattern = true)]
        [FaultContract(typeof(AppServiceFaultDetail), Name = "AppServiceFaultDetail", Namespace = "com.nordic-it.appservice.v3")]
        IAsyncResult BeginGetFolders(GetFoldersParameters parameters, AsyncCallback callback, object asyncState);

        GetFoldersResult EndGetFolders(IAsyncResult asyncResult);

        #endregion

        #region Documents module

        [OperationContract(Action = "GetDocumentPreviews", AsyncPattern = true)]
        [FaultContract(typeof(AppServiceFaultDetail), Name = "AppServiceFaultDetail", Namespace = "com.nordic-it.appservice.v3")]
        IAsyncResult BeginGetDocumentPreviews(GetDocumentPreviewsParameters parameters, AsyncCallback callback, object asyncState);

        GetDocumentPreviewsResult EndGetDocumentPreviews(IAsyncResult asyncResult);

        [OperationContract(Action = "GetDocument", AsyncPattern = true)]
        [FaultContract(typeof(AppServiceFaultDetail), Name = "AppServiceFaultDetail", Namespace = "com.nordic-it.appservice.v3")]
        IAsyncResult BeginGetDocument(GetDocumentParameters parameters, AsyncCallback callback, object asyncState);

        GetDocumentResult EndGetDocument(IAsyncResult asyncResult);

        [OperationContract(Action = "SendDocument", AsyncPattern = true)]
        [FaultContract(typeof(AppServiceFaultDetail), Name = "AppServiceFaultDetail", Namespace = "com.nordic-it.appservice.v3")]
        IAsyncResult BeginSendDocument(SendDocumentParameters parameters, AsyncCallback callback, object asyncState);

        SendDocumentResult EndSendDocument(IAsyncResult asyncResult);

        [OperationContract(Action = "SetDocumentsReadStatus", AsyncPattern = true)]
        [FaultContract(typeof(AppServiceFaultDetail), Name = "AppServiceFaultDetail", Namespace = "com.nordic-it.appservice.v3")]
        IAsyncResult BeginSetDocumentsReadStatus(SetDocumentsReadStatusParameters parameters, AsyncCallback callback, object asyncState);

        SetDocumentsReadStatusResult EndSetDocumentsReadStatus(IAsyncResult asyncResult);

        [OperationContract(Action = "SetDocumentPriority", AsyncPattern = true)]
        [FaultContract(typeof(AppServiceFaultDetail), Name = "AppServiceFaultDetail", Namespace = "com.nordic-it.appservice.v3")]
        IAsyncResult BeginSetDocumentPriority(SetDocumentPriorityParameters parameters, AsyncCallback callback, object asyncState);

        SetDocumentPriorityResult EndSetDocumentPriority(IAsyncResult asyncResult);

        [OperationContract(Action = "MoveToSpam", AsyncPattern = true)]
        [FaultContract(typeof(AppServiceFaultDetail), Name = "AppServiceFaultDetail", Namespace = "com.nordic-it.appservice.v3")]
        IAsyncResult BeginMoveToSpam(MoveToSpamParameters parameters, AsyncCallback callback, object asyncState);

        MoveToSpamResult EndMoveToSpam(IAsyncResult asyncResult);

        [OperationContract(Action = "GetTemplatePreviews", AsyncPattern = true)]
        [FaultContract(typeof(AppServiceFaultDetail), Name = "AppServiceFaultDetail", Namespace = "com.nordic-it.appservice.v3")]
        IAsyncResult BeginGetTemplatePreviews(GetTemplatePreviewsParameters parameters, AsyncCallback callback, object asyncState);

        GetTemplatePreviewsResult EndGetTemplatePreviews(IAsyncResult asyncResult);

        [OperationContract(Action = "GetTemplate", AsyncPattern = true)]
        [FaultContract(typeof(AppServiceFaultDetail), Name = "AppServiceFaultDetail", Namespace = "com.nordic-it.appservice.v3")]
        IAsyncResult BeginGetTemplate(GetTemplateParameters parameters, AsyncCallback callback, object asyncState);

        GetTemplateResult EndGetTemplate(IAsyncResult asyncResult);

        [OperationContract(Action = "GetDefaultTempalte", AsyncPattern = true)]
        [FaultContract(typeof(AppServiceFaultDetail), Name = "AppServiceFaultDetail", Namespace = "com.nordic-it.appservice.v3")]
        IAsyncResult BeginGetDefaultTemplate(GetDefaultTemplateParameters parameters, AsyncCallback callback, object asyncState);

        GetDefaultTemplateResult EndGetDefaultTemplate(IAsyncResult asyncResult);

        #endregion

        #region Contacts module

        [OperationContract(Action = "GetContactPreviews", AsyncPattern = true)]
        [FaultContract(typeof(AppServiceFaultDetail), Name = "AppServiceFaultDetail", Namespace = "com.nordic-it.appservice.v3")]
        IAsyncResult BeginGetContactPreviews(GetContactPreviewsParameters parameters, AsyncCallback callback, object asyncState);

        GetContactPreviewsResult EndGetContactPreviews(IAsyncResult asyncResult);

        [OperationContract(Action = "GetContact", AsyncPattern = true)]
        [FaultContract(typeof(AppServiceFaultDetail), Name = "AppServiceFaultDetail", Namespace = "com.nordic-it.appservice.v3")]
        IAsyncResult BeginGetContact(GetContactParameters parameters, AsyncCallback callback, object asyncState);

        GetContactResult EndGetContact(IAsyncResult asyncResult);

        [OperationContract(Action = "CreateOrUpdateContact", AsyncPattern = true)]
        [FaultContract(typeof(AppServiceFaultDetail), Name = "AppServiceFaultDetail", Namespace = "com.nordic-it.appservice.v3")]
        IAsyncResult BeginCreateOrUpdateContact(CreateOrUpdateContactParameters parameters, AsyncCallback callback, object asyncState);

        CreateOrUpdateContactResult EndCreateOrUpdateContact(IAsyncResult asyncResult);

        #endregion

        #region Shortcodes module

        [OperationContract(Action = "GetShortcodePreviews", AsyncPattern = true)]
        [FaultContract(typeof(AppServiceFaultDetail), Name = "AppServiceFaultDetail", Namespace = "com.nordic-it.appservice.v3")]
        IAsyncResult BeginGetShortcodePreviews(GetShortcodePreviewsParameters parameters, AsyncCallback callback, object asyncState);

        GetShortcodePreviewsResult EndGetShortcodePreviews(IAsyncResult asyncResult);

        [OperationContract(Action = "GetShortcode", AsyncPattern = true)]
        [FaultContract(typeof(AppServiceFaultDetail), Name = "AppServiceFaultDetail", Namespace = "com.nordic-it.appservice.v3")]
        IAsyncResult BeginGetShortcode(GetShortcodeParameters parameters, AsyncCallback callback, object asyncState);

        GetShortcodeResult EndGetShortcode(IAsyncResult asyncResult);

        #endregion

        #region Calendar module

        [OperationContract(Action = "GetCalendarEvents", AsyncPattern = true)]
        [FaultContract(typeof(AppServiceFaultDetail), Name = "AppServiceFaultDetail", Namespace = "com.nordic-it.appservice.v3")]
        IAsyncResult BeginGetCalendarEvents(GetCalendarEventsParameters parameters, AsyncCallback callback, object asyncState);

        GetCalendarEventsResult EndGetCalendarEvents(IAsyncResult asyncResult);

        [OperationContract(Action = "GetCalendarAppointment", AsyncPattern = true)]
        [FaultContract(typeof(AppServiceFaultDetail), Name = "AppServiceFaultDetail", Namespace = "com.nordic-it.appservice.v3")]
        IAsyncResult BeginGetCalendarAppointment(GetCalendarAppointmentParameters parameters, AsyncCallback callback, object asyncState);

        GetCalendarAppointmentResult EndGetCalendarAppointment(IAsyncResult asyncResult);

        [OperationContract(Action = "GetCalendarTask", AsyncPattern = true)]
        [FaultContract(typeof(AppServiceFaultDetail), Name = "AppServiceFaultDetail", Namespace = "com.nordic-it.appservice.v3")]
        IAsyncResult BeginGetCalendarTask(GetCalendarTaskParameters parameters, AsyncCallback callback, object asyncState);

        GetCalendarTaskResult EndGetCalendarTask(IAsyncResult asyncResult);

        [OperationContract(Action = "CreateOrUpdateCalendarAppointment", AsyncPattern = true)]
        [FaultContract(typeof(AppServiceFaultDetail), Name = "AppServiceFaultDetail", Namespace = "com.nordic-it.appservice.v3")]
        IAsyncResult BeginCreateOrUpdateCalendarAppointment(CreateOrUpdateCalendarAppointmentParameters parameters, AsyncCallback callback, object asyncState);

        CreateOrUpdateCalendarAppointmentResult EndCreateOrUpdateCalendarAppointment(IAsyncResult asyncResult);

        [OperationContract(Action = "CreateOrUpdateCalendarTask", AsyncPattern = true)]
        [FaultContract(typeof(AppServiceFaultDetail), Name = "AppServiceFaultDetail", Namespace = "com.nordic-it.appservice.v3")]
        IAsyncResult BeginCreateOrUpdateCalendarTask(CreateOrUpdateCalendarTaskParameters parameters, AsyncCallback callback, object asyncState);

        CreateOrUpdateCalendarTaskResult EndCreateOrUpdateCalendarTask(IAsyncResult asyncResult);

        #endregion

        #region Search

        [OperationContract(Action = "GetSavedSearches", AsyncPattern = true)]
        [FaultContract(typeof(AppServiceFaultDetail), Name = "AppServiceFaultDetail", Namespace = "com.nordic-it.appservice.v3")]
        IAsyncResult BeginGetSavedSearches(GetSavedSearchesParameters parameters, AsyncCallback callback, object asyncState);

        GetSavedSearchesResult EndGetSavedSearches(IAsyncResult asyncResult);

        [OperationContract(Action = "SearchDocuments", AsyncPattern = true)]
        [FaultContract(typeof(AppServiceFaultDetail), Name = "AppServiceFaultDetail", Namespace = "com.nordic-it.appservice.v3")]
        IAsyncResult BeginSearchDocuments(SearchDocumentsParameters parameters, AsyncCallback callback, object asyncState);

        SearchDocumentsResult EndSearchDocuments(IAsyncResult asyncResult);

        [OperationContract(Action = "SearchContacts", AsyncPattern = true)]
        [FaultContract(typeof(AppServiceFaultDetail), Name = "AppServiceFaultDetail", Namespace = "com.nordic-it.appservice.v3")]
        IAsyncResult BeginSearchContacts(SearchContactsParameters parameters, AsyncCallback callback, object asyncState);

        SearchContactsResult EndSearchContacts(IAsyncResult asyncResult);

        [OperationContract(Action = "SearchShortcodes", AsyncPattern = true)]
        [FaultContract(typeof(AppServiceFaultDetail), Name = "AppServiceFaultDetail", Namespace = "com.nordic-it.appservice.v3")]
        IAsyncResult BeginSearchShortcodes(SearchShortcodesParameters parameters, AsyncCallback callback, object asyncState);

        SearchShortcodesResult EndSearchShortcodes(IAsyncResult asyncResult);

        [OperationContract(Action = "SearchCalendarEvents", AsyncPattern = true)]
        [FaultContract(typeof(AppServiceFaultDetail), Name = "AppServiceFaultDetail", Namespace = "com.nordic-it.appservice.v3")]
        IAsyncResult BeginSearchCalendarEvents(SearchCalendarEventsParameters parameters, AsyncCallback callback, object asyncState);

        SearchCalendarEventsResult EndSearchCalendarEvents(IAsyncResult asyncResult);

        #endregion

        #region Notifications

        [OperationContract(Action = "GetNotifications", AsyncPattern = true)]
        [FaultContract(typeof(AppServiceFaultDetail), Name = "AppServiceFaultDetail", Namespace = "com.nordic-it.appservice.v3")]
        IAsyncResult BeginGetNotifications(GetNotificationsParameters parameters, AsyncCallback callback, object asyncState);

        GetNotificationsResult EndGetNotifications(IAsyncResult asyncResult);

        [OperationContract(Action = "SetFoldersNotifications", AsyncPattern = true)]
        [FaultContract(typeof(AppServiceFaultDetail), Name = "AppServiceFaultDetail", Namespace = "com.nordic-it.appservice.v3")]
        IAsyncResult BeginSetFoldersNotifications(SetFoldersNotificationsParameters parameters, AsyncCallback callback, object asyncState);

        SetFoldersNotificationsResult EndSetFoldersNotifications(IAsyncResult asyncResult);

        [OperationContract(Action = "GetFoldersNotifications", AsyncPattern = true)]
        [FaultContract(typeof(AppServiceFaultDetail), Name = "AppServiceFaultDetail", Namespace = "com.nordic-it.appservice.v3")]
        IAsyncResult BeginGetFoldersNotifications(GetFoldersNotificationsParameters parameters, AsyncCallback callback, object asyncState);

        GetFoldersNotificationsResult EndGetFoldersNotifications(IAsyncResult asyncResult);

        [OperationContract(Action = "GetCalendarNotificationsEnabled", AsyncPattern = true)]
        [FaultContract(typeof(AppServiceFaultDetail), Name = "AppServiceFaultDetail", Namespace = "com.nordic-it.appservice.v3")]
        IAsyncResult BeginGetCalendarNotificationsEnabled(GetCalendarNotificationsEnabledParameters parameters, AsyncCallback callback, object asyncState);

        GetCalendarNotificationsEnabledResult EndGetCalendarNotificationsEnabled(IAsyncResult asyncResult);

        [OperationContract(Action = "SetCalendarNotificationsEnabled", AsyncPattern = true)]
        [FaultContract(typeof(AppServiceFaultDetail), Name = "AppServiceFaultDetail", Namespace = "com.nordic-it.appservice.v3")]
        IAsyncResult BeginSetCalendarNotificationsEnabled(SetCalendarNotificationsEnabledParameters parameters, AsyncCallback callback, object asyncState);

        SetCalendarNotificationsEnabledResult EndSetCalendarNotificationsEnabled(IAsyncResult asyncResult);

        [OperationContract(Action = "GetNotificationsSound", AsyncPattern = true)]
        [FaultContract(typeof(AppServiceFaultDetail), Name = "AppServiceFaultDetail", Namespace = "com.nordic-it.appservice.v3")]
        IAsyncResult BeginGetNotificationsSound(GetNotificationsSoundParameters parameters, AsyncCallback callback, object asyncState);

        GetNotificationsSoundResult EndGetNotificationsSound(IAsyncResult asyncResult);

        [OperationContract(Action = "SetNotificationsSound", AsyncPattern = true)]
        [FaultContract(typeof(AppServiceFaultDetail), Name = "AppServiceFaultDetail", Namespace = "com.nordic-it.appservice.v3")]
        IAsyncResult BeginSetNotificationsSound(SetNotificationsSoundParameters parameters, AsyncCallback callback, object asyncState);

        SetNotificationsSoundResult EndSetNotificationsSound(IAsyncResult asyncResult);

        [OperationContract(Action = "ClearAllNotifications", AsyncPattern = true)]
        [FaultContract(typeof(AppServiceFaultDetail), Name = "AppServiceFaultDetail", Namespace = "com.nordic-it.appservice.v3")]
        IAsyncResult BeginClearAllNotifications(ClearAllNotificationsParameters parameters, AsyncCallback callback, object asyncState);

        ClearAllNotificationsResult EndClearAllNotifications(IAsyncResult asyncResult);

        #endregion

        #region Common

        [OperationContract(Action = "AddComment", AsyncPattern = true)]
        [FaultContract(typeof(AppServiceFaultDetail), Name = "AppServiceFaultDetail", Namespace = "com.nordic-it.appservice.v3")]
        IAsyncResult BeginAddComment(AddCommentParameters parameters, AsyncCallback callback, object asyncState);

        AddCommentResult EndAddComment(IAsyncResult asyncResult);

        [OperationContract(Action = "EditComment", AsyncPattern = true)]
        [FaultContract(typeof(AppServiceFaultDetail), Name = "AppServiceFaultDetail", Namespace = "com.nordic-it.appservice.v3")]
        IAsyncResult BeginEditComment(EditCommentParameters parameters, AsyncCallback callback, object asyncState);

        EditCommentResult EndEditComment(IAsyncResult asyncResult);

        [OperationContract(Action = "DeleteComment", AsyncPattern = true)]
        [FaultContract(typeof(AppServiceFaultDetail), Name = "AppServiceFaultDetail", Namespace = "com.nordic-it.appservice.v3")]
        IAsyncResult BeginDeleteComment(DeleteCommentParameters parameters, AsyncCallback callback, object asyncState);

        DeleteCommentResult EndDeleteComment(IAsyncResult asyncResult);

        [OperationContract(Action = "GetAllCategories", AsyncPattern = true)]
        [FaultContract(typeof(AppServiceFaultDetail), Name = "AppServiceFaultDetail", Namespace = "com.nordic-it.appservice.v3")]
        IAsyncResult BeginGetAllCategories(GetAllCategoriesParameters parameters, AsyncCallback callback, object asyncState);

        GetAllCategoriesResult EndGetAllCategories(IAsyncResult asyncResult);

        [OperationContract(Action = "SetCategories", AsyncPattern = true)]
        [FaultContract(typeof(AppServiceFaultDetail), Name = "AppServiceFaultDetail", Namespace = "com.nordic-it.appservice.v3")]
        IAsyncResult BeginSetCategories(SetCategoriesParameters parameters, AsyncCallback callback, object asyncState);

        SetCategoriesResult EndSetCategories(IAsyncResult asyncResult);

        [OperationContract(Action = "GetObjectActions", AsyncPattern = true)]
        [FaultContract(typeof(AppServiceFaultDetail), Name = "AppServiceFaultDetail", Namespace = "com.nordic-it.appservice.v3")]
        IAsyncResult BeginGetObjectActions(GetObjectActionsParameters parameters, AsyncCallback callback, object asyncState);

        GetObjectActionsResult EndGetObjectActions(IAsyncResult asyncResult);

        [OperationContract(Action = "GetObjectLinks", AsyncPattern = true)]
        [FaultContract(typeof(AppServiceFaultDetail), Name = "AppServiceFaultDetail", Namespace = "com.nordic-it.appservice.v3")]
        IAsyncResult BeginGetObjectLinks(GetObjectLinksParameters parameters, AsyncCallback callback, object asyncState);

        GetObjectLinksResult EndGetObjectLinks(IAsyncResult asyncResult);

        [OperationContract(Action = "GetRecentAddresses", AsyncPattern = true)]
        [FaultContract(typeof(AppServiceFaultDetail), Name = "AppServiceFaultDetail", Namespace = "com.nordic-it.appservice.v3")]
        IAsyncResult BeginGetRecentAddresses(GetRecentAddressesParameters parameters, AsyncCallback callback, object asyncState);

        GetRecentAddressesResult EndGetRecentAddresses(IAsyncResult asyncResult);

        [OperationContract(Action = "FileToFolder", AsyncPattern = true)]
        [FaultContract(typeof(AppServiceFaultDetail), Name = "AppServiceFaultDetail", Namespace = "com.nordic-it.appservice.v3")]
        IAsyncResult BeginFileToFolder(FileToFolderParameters parameters, AsyncCallback callback, object asyncState);

        FileToFolderResult EndFileToFolder(IAsyncResult asyncResult);

        [OperationContract(Action = "CopyToWorktray", AsyncPattern = true)]
        [FaultContract(typeof(AppServiceFaultDetail), Name = "AppServiceFaultDetail", Namespace = "com.nordic-it.appservice.v3")]
        IAsyncResult BeginCopyToWorktray(CopyToWorktrayParameters parameters, AsyncCallback callback, object asyncState);

        CopyToWorktrayResult EndCopyToWorktray(IAsyncResult asyncResult);

        [OperationContract(Action = "Delete", AsyncPattern = true)]
        [FaultContract(typeof(AppServiceFaultDetail), Name = "AppServiceFaultDetail", Namespace = "com.nordic-it.appservice.v3")]
        IAsyncResult BeginDelete(DeleteParameters parameters, AsyncCallback callback, object asyncState);

        DeleteResult EndDelete(IAsyncResult asyncResult);

        [OperationContract(Action = "RemoveFromFolder", AsyncPattern = true)]
        [FaultContract(typeof(AppServiceFaultDetail), Name = "AppServiceFaultDetail", Namespace = "com.nordic-it.appservice.v3")]
        IAsyncResult BeginRemoveFromFolder(RemoveFromFolderParameters parameters, AsyncCallback callback, object asyncState);

        RemoveFromFolderResult EndRemoveFromFolder(IAsyncResult asyncResult);

        #endregion

        #region System

        [OperationContract(Action = "GetSystemSettings", AsyncPattern = true)]
        [FaultContract(typeof(AppServiceFaultDetail), Name = "AppServiceFaultDetail", Namespace = "com.nordic-it.appservice.v3")]
        IAsyncResult BeginGetSystemSettings(GetSystemSettingsParameters parameters, AsyncCallback callback, object asyncState);

        GetSystemSettingsResult EndGetSystemSettings(IAsyncResult asyncResult);

        [OperationContract(Action = "GetSystemUsers", AsyncPattern = true)]
        [FaultContract(typeof(AppServiceFaultDetail), Name = "AppServiceFaultDetail", Namespace = "com.nordic-it.appservice.v3")]
        IAsyncResult BeginGetSystemUsers(GetSystemUsersParameters parameters, AsyncCallback callback, object asyncState);

        GetSystemUsersResult EndGetSystemUsers(IAsyncResult asyncResult);

        #endregion

        #region Diagnostic methods

        [OperationContract(Action = "Test", AsyncPattern = true)]
        [FaultContract(typeof(AppServiceFaultDetail), Name = "AppServiceFaultDetail", Namespace = "com.nordic-it.appservice.v3")]
        IAsyncResult BeginTest(TestParameters parameters, AsyncCallback callback, object asyncState);

        TestResult EndTest(IAsyncResult asyncResult);

        #endregion
    }
}