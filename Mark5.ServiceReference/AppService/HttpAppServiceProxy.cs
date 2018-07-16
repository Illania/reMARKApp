using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Mark5.ServiceReference.DataContract;
using Mark5.ServiceReference.Exceptions;
using Mark5.ServiceReference.Utilities;

namespace Mark5.ServiceReference.AppService
{
    class HttpAppServiceProxy : IAppServiceProxy
    {
        public Version Version => new Version(3, 0, 0);

        readonly Func<HttpMessageHandler> httpClientHandler;
        readonly Action onStartTransmission;
        readonly Action onStopTransmission;
        readonly string requestUri;

        public HttpAppServiceProxy(bool ssl, string hostname, int port, Func<HttpMessageHandler> httpClientHandler, Action onStartTransmission, Action onStopTransmission)
        {
            this.httpClientHandler = httpClientHandler;
            this.onStartTransmission = onStartTransmission;
            this.onStopTransmission = onStopTransmission;

            requestUri = $"{(ssl ? "https" : "http")}://{hostname}:{port}/app3";
        }

        async Task<R> InvokeAsync<R, P>(string soapAction, P parameters, CancellationToken ct, bool useShortTimeout = false) where R : class where P : class
        {
            HttpStatusCode statusCode = 0;
            try
            {
                onStartTransmission?.Invoke();

                using (var c = new HttpClient(httpClientHandler())
                {
                    Timeout = TimeSpan.FromSeconds(useShortTimeout ? Config.HttpClientShortTimeoutSeconds : Config.HttpClientTimeoutSeconds),
                })
                {
                    var req = CreateRequest(soapAction, parameters);
                    var res = await c.SendAsync(req, ct);
                    statusCode = res.StatusCode;
                    return await ProcessResponse<R>(soapAction, res);
                }
            }
            catch (Exception ex) when (!(ex is HttpAppServiceException))
            {
                if (ex is TaskCanceledException tce && !tce.CancellationToken.IsCancellationRequested)
                {
                    var te = new TimeoutException("Request timed out.");
                    throw new HttpAppServiceException(statusCode, te.Message, te);
                }

                throw new HttpAppServiceException(statusCode, ex.Message, ex);
            }
            finally
            {
                onStopTransmission?.Invoke();
            }
        }

        HttpRequestMessage CreateRequest<P>(string soapAction, P parameters) where P : class
        {
            var req = new HttpRequestMessage(HttpMethod.Post, requestUri);

            var dcs = new DataContractSerializer(typeof(P));
            var sw = new StringWriterWithEncoding(Encoding.UTF8);
            using (var w = XmlWriter.Create(sw, new XmlWriterSettings
            {
                OmitXmlDeclaration = false,
                Encoding = Encoding.UTF8,
                ConformanceLevel = ConformanceLevel.Document,
                NewLineHandling = NewLineHandling.None,
                NamespaceHandling = NamespaceHandling.OmitDuplicates,
                CheckCharacters = true,
                Indent = false
            }))
            {
                w.WriteStartDocument();
                w.WriteStartElement("s", "Envelope", "http://schemas.xmlsoap.org/soap/envelope/");
                w.WriteStartElement("s", "Body", "http://schemas.xmlsoap.org/soap/envelope/");
                w.WriteStartElement(typeof(P).Name.Replace("Parameters", ""), "com.nordic-it.appservice.v3");
                w.WriteStartElement("parameters");
                dcs.WriteObjectContent(w, parameters);
                w.WriteEndElement();
                w.WriteEndElement();
                w.WriteEndElement();
                w.WriteEndElement();
                w.WriteEndDocument();
            }

            var content = new StringContent(sw.ToString());
            content.Headers.Add("SOAPAction", soapAction);
            content.Headers.ContentType = new MediaTypeHeaderValue("text/xml") { CharSet = "utf-8" };
            req.Content = content;

            return req;
        }

        async Task<R> ProcessResponse<R>(string soapAction, HttpResponseMessage res) where R : class
        {
            var responseContent = await res.Content.ReadAsStringAsync();

            var doc = new XmlDocument();
            doc.LoadXml(responseContent);

            var envelope = doc.DocumentElement;
            if (envelope.LocalName != "Envelope")
                throw new SOAPException("Envelope not found.");

            var body = envelope.FirstChild;
            if (body.LocalName != "Body")
                throw new SOAPException("Body not found.");

            var response = body.FirstChild;

            if (res.StatusCode == HttpStatusCode.OK)
            {
                if (response.LocalName == typeof(R).Name.Replace("Result", "Response"))
                    return ParseResponse<R>(body);

                if (response.LocalName == "Fault")
                    throw ParseFault(body, res.StatusCode);

                throw new SOAPException($"Invalid response. {soapAction}Response or Fault not found.");
            }

            if (res.StatusCode == HttpStatusCode.InternalServerError)
            {
                if (response.LocalName == "Fault")
                    throw ParseFault(body, res.StatusCode);

                throw new SOAPException($"Invalid response. Fault not found.");
            }

            throw new HttpAppServiceException(res.StatusCode, "Invalid status code received.");
        }

        static R ParseResponse<R>(XmlNode body) where R : class
        {
            var response = body.FirstChild;

            var resultContent = response.InnerXml;

            var dcs = new DataContractSerializer(typeof(R));
            var sb = new StringReader(resultContent);
            using (var r = XmlReader.Create(sb, new XmlReaderSettings
            {
                CheckCharacters = false,
                ConformanceLevel = ConformanceLevel.Fragment
            }))
            {
                var result = dcs.ReadObject(r);
                return (R)result;
            }
        }

        static HttpAppServiceException ParseFault(XmlNode body, HttpStatusCode statusCode)
        {
            var fault = body.FirstChild;

            var faultString = fault.ChildNodes[1].InnerText;
            var faultDetailContent = fault.ChildNodes[2].InnerXml;
            AppServiceFaultDetail faultDetail = null;

            var dcs = new DataContractSerializer(typeof(AppServiceFaultDetail));
            var sb = new StringReader(faultDetailContent);
            using (var r = XmlReader.Create(sb, new XmlReaderSettings
            {
                CheckCharacters = false,
                ConformanceLevel = ConformanceLevel.Fragment
            }))
            {
                var result = dcs.ReadObject(r);
                faultDetail = (AppServiceFaultDetail)result;
            }

            return new HttpAppServiceException(statusCode, faultString, faultDetail);
        }

        public async Task<AuthenticateResult> AuthenticateAsync(AuthenticateParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            return await InvokeAsync<AuthenticateResult, AuthenticateParameters>("Authenticate", parameters, ct);
        }

        public async Task<GetFoldersResult> GetFoldersAsync(GetFoldersParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            return await InvokeAsync<GetFoldersResult, GetFoldersParameters>("GetFolders", parameters, ct);
        }

        public async Task<GetDocumentPreviewsResult> GetDocumentPreviewsAsync(GetDocumentPreviewsParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            return await InvokeAsync<GetDocumentPreviewsResult, GetDocumentPreviewsParameters>("GetDocumentPreviews", parameters, ct);
        }

        public async Task<GetDocumentResult> GetDocumentAsync(GetDocumentParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            return await InvokeAsync<GetDocumentResult, GetDocumentParameters>("GetDocument", parameters, ct);
        }

        public async Task<SendDocumentResult> SendDocumentAsync(SendDocumentParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            return await InvokeAsync<SendDocumentResult, SendDocumentParameters>("SendDocument", parameters, ct);
        }

        public async Task<SetDocumentsReadStatusResult> SetDocumentsReadStatusAsync(SetDocumentsReadStatusParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            return await InvokeAsync<SetDocumentsReadStatusResult, SetDocumentsReadStatusParameters>("SetDocumentsReadStatus", parameters, ct);
        }

        public async Task<SetDocumentPriorityResult> SetDocumentPriorityAsync(SetDocumentPriorityParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            return await InvokeAsync<SetDocumentPriorityResult, SetDocumentPriorityParameters>("SetDocumentPriority", parameters, ct);
        }

        public async Task<MoveToSpamResult> MoveToSpamAsync(MoveToSpamParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            return await InvokeAsync<MoveToSpamResult, MoveToSpamParameters>("MoveToSpam", parameters, ct);
        }

        public async Task<GetTemplatePreviewsResult> GetTemplatePreviewsAsync(GetTemplatePreviewsParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            return await InvokeAsync<GetTemplatePreviewsResult, GetTemplatePreviewsParameters>("GetTemplatePreviews", parameters, ct);
        }

        public async Task<GetTemplateResult> GetTemplateAsync(GetTemplateParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            return await InvokeAsync<GetTemplateResult, GetTemplateParameters>("GetTemplate", parameters, ct);
        }

        public async Task<GetDefaultTemplateResult> GetDefaultTemplateAsync(GetDefaultTemplateParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            return await InvokeAsync<GetDefaultTemplateResult, GetDefaultTemplateParameters>("GetDefaultTempalte", parameters, ct);
        }

        public async Task<GetLinesResult> GetLinesAsync(GetLinesParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            return await InvokeAsync<GetLinesResult, GetLinesParameters>("GetLines", parameters, ct);
        }

        public async Task<GetContactPreviewsResult> GetContactPreviewsAsync(GetContactPreviewsParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            return await InvokeAsync<GetContactPreviewsResult, GetContactPreviewsParameters>("GetContactPreviews", parameters, ct);
        }

        public async Task<GetContactResult> GetContactAsync(GetContactParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            return await InvokeAsync<GetContactResult, GetContactParameters>("GetContact", parameters, ct);
        }

        public async Task<CreateOrUpdateContactResult> CreateOrUpdateContactAsync(CreateOrUpdateContactParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            return await InvokeAsync<CreateOrUpdateContactResult, CreateOrUpdateContactParameters>("CreateOrUpdateContact", parameters, ct);
        }

        public async Task<GetShortcodePreviewsResult> GetShortcodePreviewsAsync(GetShortcodePreviewsParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            return await InvokeAsync<GetShortcodePreviewsResult, GetShortcodePreviewsParameters>("GetShortcodePreviews", parameters, ct);
        }

        public async Task<GetShortcodeResult> GetShortcodeAsync(GetShortcodeParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            return await InvokeAsync<GetShortcodeResult, GetShortcodeParameters>("GetShortcode", parameters, ct);
        }

        public async Task<CreateOrUpdateShortcodeResult> CreateOrUpdateShortcodeAsync(CreateOrUpdateShortcodeParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            return await InvokeAsync<CreateOrUpdateShortcodeResult, CreateOrUpdateShortcodeParameters>("CreateOrUpdateShortcode", parameters, ct);
        }

        public async Task<GetCalendarEventsResult> GetCalendarEventsAsync(GetCalendarEventsParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            return await InvokeAsync<GetCalendarEventsResult, GetCalendarEventsParameters>("GetCalendarEvents", parameters, ct);
        }

        public async Task<GetCalendarAppointmentResult> GetCalendarAppointmentAsync(GetCalendarAppointmentParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            return await InvokeAsync<GetCalendarAppointmentResult, GetCalendarAppointmentParameters>("GetCalendarAppointment", parameters, ct);
        }

        public async Task<GetCalendarTaskResult> GetCalendarTaskAsync(GetCalendarTaskParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            return await InvokeAsync<GetCalendarTaskResult, GetCalendarTaskParameters>("GetCalendarTask", parameters, ct);
        }

        public async Task<CreateOrUpdateCalendarAppointmentResult> CreateOrUpdateCalendarAppointmentAsync(CreateOrUpdateCalendarAppointmentParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            return await InvokeAsync<CreateOrUpdateCalendarAppointmentResult, CreateOrUpdateCalendarAppointmentParameters>("CreateOrUpdateCalendarAppointment", parameters, ct);
        }

        public async Task<CreateOrUpdateCalendarTaskResult> CreateOrUpdateCalendarTaskAsync(CreateOrUpdateCalendarTaskParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            return await InvokeAsync<CreateOrUpdateCalendarTaskResult, CreateOrUpdateCalendarTaskParameters>("CreateOrUpdateCalendarTask", parameters, ct);
        }

        public async Task<GetSavedSearchesResult> GetSavedSearchesAsync(GetSavedSearchesParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            return await InvokeAsync<GetSavedSearchesResult, GetSavedSearchesParameters>("GetSavedSearches", parameters, ct);
        }

        public async Task<SearchDocumentsResult> SearchDocumentsAsync(SearchDocumentsParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            return await InvokeAsync<SearchDocumentsResult, SearchDocumentsParameters>("SearchDocuments", parameters, ct);
        }

        public async Task<SearchContactsResult> SearchContactsAsync(SearchContactsParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            return await InvokeAsync<SearchContactsResult, SearchContactsParameters>("SearchContacts", parameters, ct);
        }

        public async Task<SearchShortcodesResult> SearchShortcodesAsync(SearchShortcodesParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            return await InvokeAsync<SearchShortcodesResult, SearchShortcodesParameters>("SearchShortcodes", parameters, ct);
        }

        public async Task<SearchCalendarEventsResult> SearchCalendarEventsAsync(SearchCalendarEventsParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            return await InvokeAsync<SearchCalendarEventsResult, SearchCalendarEventsParameters>("SearchCalendarEvents", parameters, ct);
        }

        public async Task<GetNotificationsResult> GetNotificationsAsync(GetNotificationsParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            return await InvokeAsync<GetNotificationsResult, GetNotificationsParameters>("GetNotifications", parameters, ct);
        }

        public async Task<SetFoldersNotificationsResult> SetFoldersNotificationsAsync(SetFoldersNotificationsParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            return await InvokeAsync<SetFoldersNotificationsResult, SetFoldersNotificationsParameters>("SetFoldersNotifications", parameters, ct);
        }

        public async Task<GetFoldersNotificationsResult> GetFoldersNotificationsAsync(GetFoldersNotificationsParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            return await InvokeAsync<GetFoldersNotificationsResult, GetFoldersNotificationsParameters>("GetFoldersNotifications", parameters, ct);
        }

        public async Task<GetCalendarNotificationsEnabledResult> GetCalendarNotificationsEnabledAsync(GetCalendarNotificationsEnabledParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            return await InvokeAsync<GetCalendarNotificationsEnabledResult, GetCalendarNotificationsEnabledParameters>("GetCalendarNotificationsEnabled", parameters, ct);
        }

        public async Task<SetCalendarNotificationsEnabledResult> SetCalendarNotificationsEnabledAsync(SetCalendarNotificationsEnabledParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            return await InvokeAsync<SetCalendarNotificationsEnabledResult, SetCalendarNotificationsEnabledParameters>("SetCalendarNotificationsEnabled", parameters, ct);
        }

        public async Task<GetNotificationsSoundResult> GetNotificationsSoundAsync(GetNotificationsSoundParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            return await InvokeAsync<GetNotificationsSoundResult, GetNotificationsSoundParameters>("GetNotificationsSound", parameters, ct);
        }

        public async Task<SetNotificationsSoundResult> SetNotificationsSoundAsync(SetNotificationsSoundParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            return await InvokeAsync<SetNotificationsSoundResult, SetNotificationsSoundParameters>("SetNotificationsSound", parameters, ct);
        }

        public async Task<ClearAllNotificationsResult> ClearAllNotificationsAsync(ClearAllNotificationsParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            return await InvokeAsync<ClearAllNotificationsResult, ClearAllNotificationsParameters>("ClearAllNotifications", parameters, ct);
        }

        public async Task<AddCommentResult> AddCommentAsync(AddCommentParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            return await InvokeAsync<AddCommentResult, AddCommentParameters>("AddComment", parameters, ct);
        }

        public async Task<EditCommentResult> EditCommentAsync(EditCommentParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            return await InvokeAsync<EditCommentResult, EditCommentParameters>("EditComment", parameters, ct);
        }

        public async Task<DeleteCommentResult> DeleteCommentAsync(DeleteCommentParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            return await InvokeAsync<DeleteCommentResult, DeleteCommentParameters>("DeleteComment", parameters, ct);
        }

        public async Task<GetAllCategoriesResult> GetAllCategoriesAsync(GetAllCategoriesParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            return await InvokeAsync<GetAllCategoriesResult, GetAllCategoriesParameters>("GetAllCategories", parameters, ct);
        }

        public async Task<SetCategoriesResult> SetCategoriesAsync(SetCategoriesParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            return await InvokeAsync<SetCategoriesResult, SetCategoriesParameters>("SetCategories", parameters, ct);
        }

        public async Task<GetObjectActionsResult> GetObjectActionsAsync(GetObjectActionsParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            return await InvokeAsync<GetObjectActionsResult, GetObjectActionsParameters>("GetObjectActions", parameters, ct);
        }

        public async Task<GetObjectLinksResult> GetObjectLinksAsync(GetObjectLinksParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            return await InvokeAsync<GetObjectLinksResult, GetObjectLinksParameters>("GetObjectLinks", parameters, ct);
        }

        public async Task<GetRecentAddressesResult> GetRecentAddressesAsync(GetRecentAddressesParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            return await InvokeAsync<GetRecentAddressesResult, GetRecentAddressesParameters>("GetRecentAddresses", parameters, ct);
        }

        public async Task<FileToFolderResult> FileToFolderAsync(FileToFolderParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            return await InvokeAsync<FileToFolderResult, FileToFolderParameters>("FileToFolder", parameters, ct);
        }

        public async Task<CopyToWorktrayResult> CopyToWorktrayAsync(CopyToWorktrayParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            return await InvokeAsync<CopyToWorktrayResult, CopyToWorktrayParameters>("CopyToWorktray", parameters, ct);
        }

        public async Task<DeleteResult> DeleteAsync(DeleteParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            return await InvokeAsync<DeleteResult, DeleteParameters>("Delete", parameters, ct);
        }

        public async Task<RemoveFromFolderResult> RemoveFromFolderAsync(RemoveFromFolderParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            return await InvokeAsync<RemoveFromFolderResult, RemoveFromFolderParameters>("RemoveFromFolder", parameters, ct);
        }

        public async Task<GetSystemSettingsResult> GetSystemSettingsAsync(GetSystemSettingsParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            return await InvokeAsync<GetSystemSettingsResult, GetSystemSettingsParameters>("GetSystemSettings", parameters, ct);
        }

        public async Task<GetSystemUsersResult> GetSystemUsersAsync(GetSystemUsersParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            return await InvokeAsync<GetSystemUsersResult, GetSystemUsersParameters>("GetSystemUsers", parameters, ct);
        }

        public async Task<TestResult> TestAsync(TestParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            return await InvokeAsync<TestResult, TestParameters>("Test", parameters, ct, true);
        }

        public async Task<SearchFoldersResult> SearchFolders(SearchFoldersParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            return await InvokeAsync<SearchFoldersResult, SearchFoldersParameters>("SearchFolders", parameters, ct, true);
        }
    }
}