//
// Project: Mark5.Mobile.ServiceReference
// File: AppServiceProxy.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading;
using System.Threading.Tasks;
using Mark5.ServiceReference.DataContract;
using Mark5.ServiceReference.Exceptions;

#pragma warning disable CS1701
namespace Mark5.ServiceReference.AppService
{

    class AppServiceProxy : IAppServiceProxy
    {

        public Version Version
        {
            get
            {
                return new Version(3, 0, 0);
            }
        }

        readonly Binding binding = new BasicHttpBinding
        {
            Name = "AppService.v3",
            Namespace = "com.nordic-it.appservice.v3",
            MaxBufferSize = 16 * 1024 * 1024,
            MaxReceivedMessageSize = 16 * 1024 * 1024,
            OpenTimeout = new TimeSpan(0, 15, 0),
            ReceiveTimeout = new TimeSpan(0, 15, 0),
            CloseTimeout = new TimeSpan(0, 15, 0),
            SendTimeout = new TimeSpan(0, 15, 0)
        };

        readonly Binding shortTimeoutsBinding = new BasicHttpBinding
        {
            Name = "AppService.v3",
            Namespace = "com.nordic-it.appservice.v3",
            MaxBufferSize = 16 * 1024 * 1024,
            MaxReceivedMessageSize = 16 * 1024 * 1024,
            OpenTimeout = new TimeSpan(0, 2, 0),
            ReceiveTimeout = new TimeSpan(0, 2, 0),
            CloseTimeout = new TimeSpan(0, 2, 0),
            SendTimeout = new TimeSpan(0, 2, 0)
        };

        readonly EndpointAddress endpoint;

        public AppServiceProxy(bool ssl, string hostname, int port)
        {
            endpoint = new EndpointAddress($"{(ssl ? "https" : "http")}://{hostname}:{port}/app3");
        }

        #region Authentication

        public async Task<AuthenticationResult> AuthenticateAsync(AuthenticationParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            var c = GetClient();
            var result = await InvokeAsync(c, c.BeginAuthenticate, c.EndAuthenticate, parameters, ct);
            c = null;
            return result;
        }

        #endregion

        #region Folders

        public async Task<GetFoldersResult> GetFoldersAsync(GetFoldersParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            var c = GetClient();
            var result = await InvokeAsync(c, c.BeginGetFolders, c.EndGetFolders, parameters, ct);
            c = null;
            return result;
        }

        #endregion

        #region Documents module

        public async Task<GetDocumentPreviewsResult> GetDocumentPreviewsAsync(GetDocumentPreviewsParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            var c = GetClient();
            var result = await InvokeAsync(c, c.BeginGetDocumentPreviews, c.EndGetDocumentPreviews, parameters, ct);
            c = null;
            return result;
        }

        public async Task<GetDocumentResult> GetDocumentAsync(GetDocumentParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            var c = GetClient();
            var result = await InvokeAsync(c, c.BeginGetDocument, c.EndGetDocument, parameters, ct);
            c = null;
            return result;
        }

        public async Task<SendDocumentResult> SendDocumentAsync(SendDocumentParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            var c = GetClient();
            var result = await InvokeAsync(c, c.BeginSendDocument, c.EndSendDocument, parameters, ct);
            c = null;
            return result;
        }

        public async Task<SetDocumentsReadStatusResult> SetDocumentsReadStatusAsync(SetDocumentsReadStatusParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            var c = GetClient();
            var result = await InvokeAsync(c, c.BeginSetDocumentsReadStatus, c.EndSetDocumentsReadStatus, parameters, ct);
            c = null;
            return result;
        }

        public async Task<SetDocumentPriorityResult> SetDocumentPriorityAsync(SetDocumentPriorityParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            var c = GetClient();
            var result = await InvokeAsync(c, c.BeginSetDocumentPriority, c.EndSetDocumentPriority, parameters, ct);
            c = null;
            return result;
        }

        public async Task<MoveToSpamResult> MoveToSpamAsync(MoveToSpamParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            var c = GetClient();
            var result = await InvokeAsync(c, c.BeginMoveToSpam, c.EndMoveToSpam, parameters, ct);
            c = null;
            return result;
        }

        public async Task<GetTemplatePreviewsResult> GetTemplatePreviewsAsync(GetTemplatePreviewsParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            var c = GetClient();
            var result = await InvokeAsync(c, c.BeginGetTemplatePreviews, c.EndGetTemplatePreviews, parameters, ct);
            c = null;
            return result;
        }

        public async Task<GetTemplateResult> GetTemplateAsync(GetTemplateParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            var c = GetClient();
            var result = await InvokeAsync(c, c.BeginGetTemplate, c.EndGetTemplate, parameters, ct);
            c = null;
            return result;
        }

        public async Task<GetDefaultTemplateResult> GetDefaultTemplateAsync(GetDefaultTemplateParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            var c = GetClient();
            var result = await InvokeAsync(c, c.BeginGetDefaultTemplate, c.EndGetDefaultTemplate, parameters, ct);
            c = null;
            return result;
        }

        #endregion

        #region Contacts module

        public async Task<GetContactPreviewsResult> GetContactPreviewsAsync(GetContactPreviewsParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            var c = GetClient();
            var result = await InvokeAsync(c, c.BeginGetContactPreviews, c.EndGetContactPreviews, parameters, ct);
            c = null;
            return result;
        }

        public async Task<GetContactResult> GetContactAsync(GetContactParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            var c = GetClient();
            var result = await InvokeAsync(c, c.BeginGetContact, c.EndGetContact, parameters, ct);
            c = null;
            return result;
        }

        public async Task<CreateOrUpdateContactResult> CreateOrUpdateContactAsync(CreateOrUpdateContactParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            var c = GetClient();
            var result = await InvokeAsync(c, c.BeginCreateOrUpdateContact, c.EndCreateOrUpdateContact, parameters, ct);
            c = null;
            return result;
        }

        #endregion

        #region Shortcodes module

        public async Task<GetShortcodePreviewsResult> GetShortcodePreviewsAsync(GetShortcodePreviewsParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            var c = GetClient();
            var result = await InvokeAsync(c, c.BeginGetShortcodePreviews, c.EndGetShortcodePreviews, parameters, ct);
            c = null;
            return result;
        }

        public async Task<GetShortcodeResult> GetShortcodeAsync(GetShortcodeParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            var c = GetClient();
            var result = await InvokeAsync(c, c.BeginGetShortcode, c.EndGetShortcode, parameters, ct);
            c = null;
            return result;
        }

        #endregion

        #region Calendar module

        public async Task<GetCalendarEventsResult> GetCalendarEventsAsync(GetCalendarEventsParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            var c = GetClient();
            var result = await InvokeAsync(c, c.BeginGetCalendarEvents, c.EndGetCalendarEvents, parameters, ct);
            c = null;
            return result;
        }

        public async Task<GetCalendarAppointmentResult> GetCalendarAppointmentAsync(GetCalendarAppointmentParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            var c = GetClient();
            var result = await InvokeAsync(c, c.BeginGetCalendarAppointment, c.EndGetCalendarAppointment, parameters, ct);
            c = null;
            return result;
        }

        public async Task<GetCalendarTaskResult> GetCalendarTaskAsync(GetCalendarTaskParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            var c = GetClient();
            var result = await InvokeAsync(c, c.BeginGetCalendarTask, c.EndGetCalendarTask, parameters, ct);
            c = null;
            return result;
        }

        public async Task<CreateOrUpdateCalendarAppointmentResult> CreateOrUpdateCalendarAppointmentAsync(CreateOrUpdateCalendarAppointmentParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            var c = GetClient();
            var result = await InvokeAsync(c, c.BeginCreateOrUpdateCalendarAppointment, c.EndCreateOrUpdateCalendarAppointment, parameters, ct);
            c = null;
            return result;
        }

        public async Task<CreateOrUpdateCalendarTaskResult> CreateOrUpdateCalendarTaskAsync(CreateOrUpdateCalendarTaskParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            var c = GetClient();
            var result = await InvokeAsync(c, c.BeginCreateOrUpdateCalendarTask, c.EndCreateOrUpdateCalendarTask, parameters, ct);
            c = null;
            return result;
        }

        #endregion

        #region Search

        public async Task<GetSavedSearchesResult> GetSavedSearchesAsync(GetSavedSearchesParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            var c = GetClient();
            var result = await InvokeAsync(c, c.BeginGetSavedSearches, c.EndGetSavedSearches, parameters, ct);
            c = null;
            return result;
        }

        public async Task<SearchDocumentsResult> SearchDocumentsAsync(SearchDocumentsParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            var c = GetClient();
            var result = await InvokeAsync(c, c.BeginSearchDocuments, c.EndSearchDocuments, parameters, ct);
            c = null;
            return result;
        }

        public async Task<SearchContactsResult> SearchContactsAsync(SearchContactsParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            var c = GetClient();
            var result = await InvokeAsync(c, c.BeginSearchContacts, c.EndSearchContacts, parameters, ct);
            c = null;
            return result;
        }

        public async Task<SearchShortcodesResult> SearchShortcodesAsync(SearchShortcodesParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            var c = GetClient();
            var result = await InvokeAsync(c, c.BeginSearchShortcodes, c.EndSearchShortcodes, parameters, ct);
            c = null;
            return result;
        }

        public async Task<SearchCalendarEventsResult> SearchCalendarEventsAsync(SearchCalendarEventsParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            var c = GetClient();
            var result = await InvokeAsync(c, c.BeginSearchCalendarEvents, c.EndSearchCalendarEvents, parameters, ct);
            c = null;
            return result;
        }

        #endregion

        #region Notifications

        public async Task<GetNotificationsResult> GetNotificationsAsync(GetNotificationsParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            var c = GetClient();
            var result = await InvokeAsync(c, c.BeginGetNotifications, c.EndGetNotifications, parameters, ct);
            c = null;
            return result;
        }

        public async Task<SetFoldersNotificationsResult> SetFoldersNotificationsAsync(SetFoldersNotificationsParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            var c = GetClient();
            var result = await InvokeAsync(c, c.BeginSetFoldersNotifications, c.EndSetFoldersNotifications, parameters, ct);
            c = null;
            return result;
        }

        public async Task<GetFoldersNotificationsResult> GetFoldersNotificationsAsync(GetFoldersNotificationsParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            var c = GetClient();
            var result = await InvokeAsync(c, c.BeginGetFoldersNotifications, c.EndGetFoldersNotifications, parameters, ct);
            c = null;
            return result;
        }

        public async Task<GetCalendarNotificationsEnabledResult> GetCalendarNotificationsEnabledAsync(GetCalendarNotificationsEnabledParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            var c = GetClient();
            var result = await InvokeAsync(c, c.BeginGetCalendarNotificationsEnabled, c.EndGetCalendarNotificationsEnabled, parameters, ct);
            c = null;
            return result;
        }

        public async Task<SetCalendarNotificationsEnabledResult> SetCalendarNotificationsEnabledAsync(SetCalendarNotificationsEnabledParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            var c = GetClient();
            var result = await InvokeAsync(c, c.BeginSetCalendarNotificationsEnabled, c.EndSetCalendarNotificationsEnabled, parameters, ct);
            c = null;
            return result;
        }

        public async Task<GetNotificationsSoundResult> GetNotificationsSoundAsync(GetNotificationsSoundParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            var c = GetClient();
            var result = await InvokeAsync(c, c.BeginGetNotificationsSound, c.EndGetNotificationsSound, parameters, ct);
            c = null;
            return result;
        }

        public async Task<SetNotificationsSoundResult> SetNotificationsSoundAsync(SetNotificationsSoundParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            var c = GetClient();
            var result = await InvokeAsync(c, c.BeginSetNotificationsSound, c.EndSetNotificationsSound, parameters, ct);
            c = null;
            return result;
        }

        public async Task<ClearAllNotificationsResult> ClearAllNotificationsAsync(ClearAllNotificationsParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            var c = GetClient();
            var result = await InvokeAsync(c, c.BeginClearAllNotifications, c.EndClearAllNotifications, parameters, ct);
            c = null;
            return result;
        }

        #endregion

        #region Common

        public async Task<AddCommentResult> AddCommentAsync(AddCommentParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            var c = GetClient();
            var result = await InvokeAsync(c, c.BeginAddComment, c.EndAddComment, parameters, ct);
            c = null;
            return result;
        }

        public async Task<EditCommentResult> EditCommentAsync(EditCommentParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            var c = GetClient();
            var result = await InvokeAsync(c, c.BeginEditComment, c.EndEditComment, parameters, ct);
            c = null;
            return result;
        }

        public async Task<DeleteCommentResult> DeleteCommentAsync(DeleteCommentParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            var c = GetClient();
            var result = await InvokeAsync(c, c.BeginDeleteComment, c.EndDeleteComment, parameters, ct);
            c = null;
            return result;
        }

        public async Task<GetAllCategoriesResult> GetAllCategoriesAsync(GetAllCategoriesParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            var c = GetClient();
            var result = await InvokeAsync(c, c.BeginGetAllCategories, c.EndGetAllCategories, parameters, ct);
            c = null;
            return result;
        }

        public async Task<SetCategoriesResult> SetCategoriesAsync(SetCategoriesParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            var c = GetClient();
            var result = await InvokeAsync(c, c.BeginSetCategories, c.EndSetCategories, parameters, ct);
            c = null;
            return result;
        }

        public async Task<GetObjectActionsResult> GetObjectActionsAsync(GetObjectActionsParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            var c = GetClient();
            var result = await InvokeAsync(c, c.BeginGetObjectActions, c.EndGetObjectActions, parameters, ct);
            c = null;
            return result;
        }

        public async Task<GetObjectLinksResult> GetObjectLinksAsync(GetObjectLinksParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            var c = GetClient();
            var result = await InvokeAsync(c, c.BeginGetObjectLinks, c.EndGetObjectLinks, parameters, ct);
            c = null;
            return result;
        }

        public async Task<GetRecentAddressesResult> GetRecentAddressesAsync(GetRecentAddressesParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            var c = GetClient();
            var result = await InvokeAsync(c, c.BeginGetRecentAddresses, c.EndGetRecentAddresses, parameters, ct);
            c = null;
            return result;
        }

        public async Task<FileToFolderResult> FileToFolderAsync(FileToFolderParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            var c = GetClient();
            var result = await InvokeAsync(c, c.BeginFileToFolder, c.EndFileToFolder, parameters, ct);
            c = null;
            return result;
        }

        public async Task<CopyToWorktrayResult> CopyToWorktrayAsync(CopyToWorktrayParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            var c = GetClient();
            var result = await InvokeAsync(c, c.BeginCopyToWorktray, c.EndCopyToWorktray, parameters, ct);
            c = null;
            return result;
        }

        public async Task<DeleteResult> DeleteAsync(DeleteParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            var c = GetClient();
            var result = await InvokeAsync(c, c.BeginDelete, c.EndDelete, parameters, ct);
            c = null;
            return result;
        }

        public async Task<RemoveFromFolderResult> RemoveFromFolderAsync(RemoveFromFolderParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            var c = GetClient();
            var result = await InvokeAsync(c, c.BeginRemoveFromFolder, c.EndRemoveFromFolder, parameters, ct);
            c = null;
            return result;
        }

        #endregion

        #region System

        public async Task<GetSystemSettingsResult> GetSystemSettingsAsync(GetSystemSettingsParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            var c = GetClient();
            var result = await InvokeAsync(c, c.BeginGetSystemSettings, c.EndGetSystemSettings, parameters, ct);
            c = null;
            return result;
        }

        public async Task<GetSystemUsersResult> GetSystemUsersAsync(GetSystemUsersParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            var c = GetClient();
            var result = await InvokeAsync(c, c.BeginGetSystemUsers, c.EndGetSystemUsers, parameters, ct);
            c = null;
            return result;
        }

        #endregion

        #region Diagnostic methods

        public async Task<TestResult> TestAsync(TestParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            var c = GetClient(true);
            var result = await InvokeAsync(c, c.BeginTest, c.EndTest, parameters, ct);
            c = null;
            return result;
        }

        #endregion

        #region Helpers

        AppServiceClient GetClient(bool shortTimeouts = false)
        {
            return new AppServiceClient(shortTimeouts ? shortTimeoutsBinding : binding, endpoint);
        }

        async Task<TResult> InvokeAsync<TResult, TParameter>(AppServiceClient client, Func<TParameter, AsyncCallback, object, IAsyncResult> beginMethod,
                                                             Func<IAsyncResult, TResult> endMethod, TParameter parameters, CancellationToken ct = default(CancellationToken))
        {
            var success = false;
            try
            {
                ct.ThrowIfCancellationRequested();

                var result = await Task.Factory.FromAsync(beginMethod, endMethod, parameters, null);
                ((ICommunicationObject)client).Close();
                success = true;
                return result;
            }
            catch (Exception ex) when (ex is FaultException || ex is FaultException<AppServiceFaultCode>)
            {
                try
                {
                    ((ICommunicationObject)client)?.Close();
                }
                catch
                {
                    try
                    {
                        ((ICommunicationObject)client)?.Abort();
                    }
                    catch
                    {
                        // Nothing to do here
                    }
                }
                throw new AppServiceException(ex);
            }
            catch (Exception ex)
            {
                if (!success)
                {
                    try
                    {
                        ((ICommunicationObject)client)?.Abort();
                    }
                    catch
                    {
                        // Nothing to do here
                    }
                }
                throw new AppServiceException(ex);
            }
        }

        #endregion

    }
}

