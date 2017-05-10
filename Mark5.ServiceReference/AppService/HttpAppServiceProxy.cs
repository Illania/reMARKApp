//
// Project: Mark5.Mobile.ServiceReference
// File: WcfAppServiceProxy.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Mark5.ServiceReference.DataContract;

#pragma warning disable CS1701
namespace Mark5.ServiceReference.AppService
{

    class HttpAppServiceProxy : IAppServiceProxy
    {

        public Version Version { get { return new Version(3, 0, 0); } }

        readonly Func<HttpMessageHandler> httpClientHandler;
        readonly string requestUri;

        public HttpAppServiceProxy(bool ssl, string hostname, int port, Func<HttpMessageHandler> httpClientHandler)
        {
            this.httpClientHandler = httpClientHandler;

            requestUri = $"{(ssl ? "https" : "http")}://{hostname}:{port}/app3";
        }

        async Task<R> InvokeAsync<R, P>(string soapAction, P parameters, CancellationToken ct) where R : class where P : class
        {
            using (var c = new HttpClient(httpClientHandler()))
            {
                // Request
                var req = new HttpRequestMessage(HttpMethod.Post, requestUri);

                var doc = new XmlDocument();
                var declaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
                var root = doc.DocumentElement;
                doc.InsertBefore(declaration, root);

                var envelope = doc.CreateElement("s", "Envelope", "http://schemas.xmlsoap.org/soap/envelope/");
                doc.AppendChild(envelope);

                var body = doc.CreateElement("s", "Body", "http://schemas.xmlsoap.org/soap/envelope/");
                envelope.AppendChild(body);

                var dcs = new DataContractSerializer(typeof(P));
                var sb = new StringBuilder();
                using (var writer = XmlWriter.Create(sb, new XmlWriterSettings { NamespaceHandling = NamespaceHandling.OmitDuplicates, OmitXmlDeclaration = true }))
                {
                    writer.WriteStartElement(soapAction, "com.nordic-it.appservice.v3");
                    writer.WriteStartElement("parameters");
                    dcs.WriteObjectContent(writer, parameters);
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                }

                body.InnerXml = sb.ToString();

                var content = new StringContent(doc.OuterXml);
                content.Headers.Add("SOAPAction", soapAction);
                content.Headers.ContentType = new MediaTypeHeaderValue("text/xml") { CharSet = "utf-8" };
                req.Content = content;

                var res = await c.SendAsync(req);

                // Response
                var responseContent = await res.Content.ReadAsStringAsync();


                var doc2 = new XmlDocument();
                doc2.LoadXml(responseContent);
                var root2 = doc2.DocumentElement;

                var envelope2 = root2.FirstChild;
                var responseelement = envelope2.FirstChild;

                var dcs2 = new DataContractSerializer(typeof(R));
                var sb2 = new StringReader(responseelement.InnerXml);
                using (var reader = XmlReader.Create(sb2))
                {
                    var result = dcs.ReadObject(reader);

                    return (R)result;
                }
            }
        }

        #region Authentication

        public async Task<AuthenticationResult> AuthenticateAsync(AuthenticationParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            return await InvokeAsync<AuthenticationResult, AuthenticationParameters>("Authenticate", parameters, ct);
        }

        #endregion

        #region Folders

        public async Task<GetFoldersResult> GetFoldersAsync(GetFoldersParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            return await InvokeAsync<GetFoldersResult, GetFoldersParameters>("GetFolders", parameters, ct);
        }

        #endregion

        public Task<GetDocumentPreviewsResult> GetDocumentPreviewsAsync(GetDocumentPreviewsParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task<GetDocumentResult> GetDocumentAsync(GetDocumentParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task<SendDocumentResult> SendDocumentAsync(SendDocumentParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task<SetDocumentsReadStatusResult> SetDocumentsReadStatusAsync(SetDocumentsReadStatusParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task<SetDocumentPriorityResult> SetDocumentPriorityAsync(SetDocumentPriorityParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task<MoveToSpamResult> MoveToSpamAsync(MoveToSpamParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task<GetTemplatePreviewsResult> GetTemplatePreviewsAsync(GetTemplatePreviewsParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task<GetTemplateResult> GetTemplateAsync(GetTemplateParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task<GetDefaultTemplateResult> GetDefaultTemplateAsync(GetDefaultTemplateParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task<GetContactPreviewsResult> GetContactPreviewsAsync(GetContactPreviewsParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task<GetContactResult> GetContactAsync(GetContactParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task<CreateOrUpdateContactResult> CreateOrUpdateContactAsync(CreateOrUpdateContactParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task<GetShortcodePreviewsResult> GetShortcodePreviewsAsync(GetShortcodePreviewsParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task<GetShortcodeResult> GetShortcodeAsync(GetShortcodeParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task<GetCalendarEventsResult> GetCalendarEventsAsync(GetCalendarEventsParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task<GetCalendarAppointmentResult> GetCalendarAppointmentAsync(GetCalendarAppointmentParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task<GetCalendarTaskResult> GetCalendarTaskAsync(GetCalendarTaskParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task<CreateOrUpdateCalendarAppointmentResult> CreateOrUpdateCalendarAppointmentAsync(CreateOrUpdateCalendarAppointmentParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task<CreateOrUpdateCalendarTaskResult> CreateOrUpdateCalendarTaskAsync(CreateOrUpdateCalendarTaskParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task<GetSavedSearchesResult> GetSavedSearchesAsync(GetSavedSearchesParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task<SearchDocumentsResult> SearchDocumentsAsync(SearchDocumentsParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task<SearchContactsResult> SearchContactsAsync(SearchContactsParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task<SearchShortcodesResult> SearchShortcodesAsync(SearchShortcodesParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task<SearchCalendarEventsResult> SearchCalendarEventsAsync(SearchCalendarEventsParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task<GetNotificationsResult> GetNotificationsAsync(GetNotificationsParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task<SetFoldersNotificationsResult> SetFoldersNotificationsAsync(SetFoldersNotificationsParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task<GetFoldersNotificationsResult> GetFoldersNotificationsAsync(GetFoldersNotificationsParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task<GetCalendarNotificationsEnabledResult> GetCalendarNotificationsEnabledAsync(GetCalendarNotificationsEnabledParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task<SetCalendarNotificationsEnabledResult> SetCalendarNotificationsEnabledAsync(SetCalendarNotificationsEnabledParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task<GetNotificationsSoundResult> GetNotificationsSoundAsync(GetNotificationsSoundParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task<SetNotificationsSoundResult> SetNotificationsSoundAsync(SetNotificationsSoundParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task<ClearAllNotificationsResult> ClearAllNotificationsAsync(ClearAllNotificationsParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task<AddCommentResult> AddCommentAsync(AddCommentParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task<EditCommentResult> EditCommentAsync(EditCommentParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task<DeleteCommentResult> DeleteCommentAsync(DeleteCommentParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task<GetAllCategoriesResult> GetAllCategoriesAsync(GetAllCategoriesParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task<SetCategoriesResult> SetCategoriesAsync(SetCategoriesParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task<GetObjectActionsResult> GetObjectActionsAsync(GetObjectActionsParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task<GetObjectLinksResult> GetObjectLinksAsync(GetObjectLinksParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task<GetRecentAddressesResult> GetRecentAddressesAsync(GetRecentAddressesParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task<FileToFolderResult> FileToFolderAsync(FileToFolderParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task<CopyToWorktrayResult> CopyToWorktrayAsync(CopyToWorktrayParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task<DeleteResult> DeleteAsync(DeleteParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task<RemoveFromFolderResult> RemoveFromFolderAsync(RemoveFromFolderParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task<GetSystemSettingsResult> GetSystemSettingsAsync(GetSystemSettingsParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task<GetSystemUsersResult> GetSystemUsersAsync(GetSystemUsersParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task<TestResult> TestAsync(TestParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

    }
}

