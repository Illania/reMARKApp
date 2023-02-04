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
using Mark5.Mobile.Classes.AuthService;
using Mark5.Mobile.Classes.Azure;
using Mark5.ServiceReference.DataContract;
using Mark5.ServiceReference.Exceptions;
using Mark5.ServiceReference.Utilities;
using JwtDecoder = Mark5.Mobile.Classes.JwtDecoder;
using Mark5.Mobile.Classes;
using Polly;
using Polly.Bulkhead;
using Polly.Wrap;

namespace Mark5.ServiceReference.AppService
{
    class HttpAppServiceProxy : IAppServiceProxy
    {
        public Version Version => new Version(3, 0, 0);

        readonly Func<HttpMessageHandler> httpClientHandler;
        readonly Action onStartTransmission;
        readonly Action onStopTransmission;
        readonly string requestUri;
        string bearerToken;
        readonly AzureApplicationProxyInfo azureApplicationProxyInfo;
        readonly AsyncPolicyWrap policy;
        readonly IReachability reachability;
        const int attempts = 3;
        const double timeOut = 200;


        public HttpAppServiceProxy(bool ssl, string hostname, string port, Func<HttpMessageHandler> httpClientHandler,
            Action onStartTransmission, Action onStopTransmission, IReachability reachability,
            string bearerToken, AzureApplicationProxyInfo azureApplicationProxyInfo)
        {
     
            this.httpClientHandler = httpClientHandler;
            this.onStartTransmission = onStartTransmission;
            this.onStopTransmission = onStopTransmission;
            this.reachability = reachability;
            this.bearerToken = bearerToken;
            this.azureApplicationProxyInfo = azureApplicationProxyInfo;

            var usePort = !string.IsNullOrEmpty(port);

            requestUri = $"{(ssl ? "https" : "http")}://{hostname}{(usePort ? (":" + port) : "")}/app3";

            if (!string.IsNullOrEmpty(bearerToken))
            {
                AzureSettings.AccessToken = bearerToken;
                AzureSettings.AppClientId = azureApplicationProxyInfo.AppClientId ?? string.Empty;
                AzureSettings.AppProxyId = azureApplicationProxyInfo.ApplicationProxyClientId ?? string.Empty;
                AzureSettings.IsEnabled = azureApplicationProxyInfo.IsEnabled;
            }
            
        }

        async Task<R> InvokeAsync<R, P>(string soapAction, P parameters, CancellationToken ct, bool useShortTimeout = false,
                                           bool checkXmlCharacters = true) where R : class where P : class
        {
            HttpStatusCode statusCode = 0;
            var useBearerToken = !string.IsNullOrEmpty(bearerToken);

            async Task<R> CreateRequestAsync()
            {
                using var c = new HttpClient(httpClientHandler())
                {
                    Timeout = TimeSpan.FromSeconds(useShortTimeout ? Config.HttpClientShortTimeoutSeconds : Config.HttpClientTimeoutSeconds)
                };
                var req = CreateRequest(soapAction, parameters, checkXmlCharacters, bearerToken);
                var res = useBearerToken ? await c.SendAsync(req) : await c.SendAsync(req, ct);
                statusCode = res.StatusCode;
                return await ProcessResponse<R>(soapAction, res);
            }

            async Task RefreshAzureToken()
            {
                if (ShouldRefreshBearerToken(azureApplicationProxyInfo, bearerToken))
                {
                    var azureAppProxyAuthService = new AzureAppProxyAuthService(azureApplicationProxyInfo.AppClientId,
                       azureApplicationProxyInfo.ApplicationProxyClientId);
                    bearerToken = await azureAppProxyAuthService.Authenticate(this, false, true);

                    if (!string.IsNullOrEmpty(bearerToken))
                    {
                        AzureSettings.AccessToken = bearerToken;
                        AzureSettings.AccessTokenLastUpdated = DateTime.Now.ToLocalTime();
                    }
                }
            }

            AsyncPolicy GetRetryPolicy()
            {
                return Policy.Handle<Exception>()
                    .WaitAndRetryAsync(attempts, attempt => TimeSpan.FromMilliseconds(timeOut), (exception, calculatedWaitDuration) => { });
            }

            var policy = GetRetryPolicy();

            try
            {
                onStartTransmission?.Invoke();

                await RefreshAzureToken();

                return await policy.ExecuteAsync(async () =>
                {
                    return await CreateRequestAsync();
                });

            }
            catch (Exception ex) when (!(ex is HttpAppServiceException))
            {
                if (ex is TaskCanceledException tce && !tce.CancellationToken.IsCancellationRequested)
                {
                    var te = new TimeoutException("Request timed out.");
                    throw new HttpAppServiceException(statusCode, te.Message, te);
                }
                if(ex is System.Net.WebException we && statusCode == 0)
                {
                    reachability.RefreshServiceReachability(false);
                    await reachability.Refresh();
                }
                throw new HttpAppServiceException(statusCode, ex.Message, ex); 
            }
            finally
            {
                onStopTransmission?.Invoke(); 
            }  
        }

        bool ShouldRefreshBearerToken(AzureApplicationProxyInfo azureApplicationProxyInfo, string bearerToken)
        {
            return azureApplicationProxyInfo != null && azureApplicationProxyInfo.IsValid()
                     && !string.IsNullOrEmpty(bearerToken) && AzureSettings.IsTokenCloseToExpire();
        }


        HttpRequestMessage CreateRequest<P>(string soapAction, P parameters, bool checkXmlCharacters = true, string bearerToken = "") where P : class
        {
            var req = new HttpRequestMessage(HttpMethod.Post, requestUri);
            if(!string.IsNullOrEmpty(bearerToken))
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            var dcs = new DataContractSerializer(typeof(P));
            var sw = new StringWriterWithEncoding(Encoding.UTF8);
            using (var w = XmlWriter.Create(sw, new XmlWriterSettings
            {
                OmitXmlDeclaration = false,
                Encoding = Encoding.UTF8,
                ConformanceLevel = ConformanceLevel.Document,
                NewLineHandling = NewLineHandling.None,
                NamespaceHandling = NamespaceHandling.OmitDuplicates,
                CheckCharacters = checkXmlCharacters,
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

        public async Task<AuthenticateWithAzureResult> AuthenticateWithAzureAsync(AuthenticateWithAzureParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            return await InvokeAsync<AuthenticateWithAzureResult, AuthenticateWithAzureParameters>("AuthenticateWithAzure", parameters, ct);
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
            return await InvokeAsync<SendDocumentResult, SendDocumentParameters>("SendDocument", parameters, ct, checkXmlCharacters: false);
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

        public async Task<ReplyToCalendarInvitationResult> ReplyToCalendarInvitationAsync(ReplyToCalendarInvitationParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            return await InvokeAsync<ReplyToCalendarInvitationResult, ReplyToCalendarInvitationParameters>("ReplyToCalendarInvitation", parameters, ct);
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

        public async Task<GetCalendarAppointmentsResult> GetCalendarAppointmentsAsync(GetCalendarAppointmentsParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            return await InvokeAsync<GetCalendarAppointmentsResult, GetCalendarAppointmentsParameters>("GetCalendarAppointments", parameters, ct);
        }

        public async Task<GetCalendarAppointmentResult> GetCalendarAppointmentAsync(GetCalendarAppointmentParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            return await InvokeAsync<GetCalendarAppointmentResult, GetCalendarAppointmentParameters>("GetCalendarAppointment", parameters, ct);
        }

        public async Task<GetCalendarAppointmentOccurrencesResult> GetCalendarAppointmentOccurrencesAsync(GetCalendarAppointmentOccurrencesParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            return await InvokeAsync<GetCalendarAppointmentOccurrencesResult, GetCalendarAppointmentOccurrencesParameters>("GetCalendarAppointmentOccurrences", parameters, ct);
        }

        public async Task<CreateOrUpdateCalendarAppointmentResult> CreateOrUpdateCalendarAppointmentAsync(CreateOrUpdateCalendarAppointmentParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            return await InvokeAsync<CreateOrUpdateCalendarAppointmentResult, CreateOrUpdateCalendarAppointmentParameters>("CreateOrUpdateCalendarAppointment", parameters, ct);
        }

        public async Task<DeleteCalendarAppointmentResult> DeleteCalendarAppointmentAsync(DeleteCalendarAppointmentParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            return await InvokeAsync<DeleteCalendarAppointmentResult, DeleteCalendarAppointmentParameters>("DeleteCalendarAppointment", parameters, ct);
        }

        public async Task<SendCalendarAppointmentInvitationsResult> SendCalendarAppointmentInvitationsAsync(SendCalendarAppointmentInvitationsParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            return await InvokeAsync<SendCalendarAppointmentInvitationsResult, SendCalendarAppointmentInvitationsParameters>("SendCalendarAppointmentInvitations", parameters, ct);
        }

        public async Task<GetCalendarAlarmsResult> GetCalendarAlarms(GetCalendarAlarmsParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            return await InvokeAsync<GetCalendarAlarmsResult, GetCalendarAlarmsParameters>("GetCalendarAlarms", parameters, ct);
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

        public async Task<GetNotificationsResult> GetNotificationsAsync(GetNotificationsParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            return await InvokeAsync<GetNotificationsResult, GetNotificationsParameters>("GetNotifications", parameters, ct);
        }

        public async Task<SetNotificationReadStatusResult> SetNotificationReadStatusAsync(SetNotificationReadStatusParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            return await InvokeAsync<SetNotificationReadStatusResult, SetNotificationReadStatusParameters>("SetNotificationReadStatus", parameters, ct);
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

        public async Task<GetFavoriteCategoriesResult> GetFavoriteCategoriesAsync(GetFavoriteCategoriesParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            return await InvokeAsync<GetFavoriteCategoriesResult, GetFavoriteCategoriesParameters>("GetFavoriteCategories", parameters, ct);
        }

        public async Task<AddFavoriteCategoryResult> AddFavoriteCategoryAsync(AddFavoriteCategoryParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            return await InvokeAsync<AddFavoriteCategoryResult, AddFavoriteCategoryParameters>("AddFavoriteCategory", parameters, ct);
        }

        public async Task<RemoveFavoriteCategoryResult> RemoveFavoriteCategoryAsync(RemoveFavoriteCategoryParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            return await InvokeAsync<RemoveFavoriteCategoryResult, RemoveFavoriteCategoryParameters>("RemoveFavoriteCategory", parameters, ct);
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

        public async Task<DeleteRecentAddressesResult> DeleteRecentAddressesAsync(DeleteRecentAddressesParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            return await InvokeAsync<DeleteRecentAddressesResult, DeleteRecentAddressesParameters>("DeleteRecentAddresses", parameters, ct);
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

        public async Task<GetFavoriteFoldersResult> GetFavoriteFolders(GetFavoriteFoldersParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            return await InvokeAsync<GetFavoriteFoldersResult, GetFavoriteFoldersParameters>("GetFavoriteFolders", parameters, ct);
        }

        public async Task<UpdateFavoriteFoldersResult> UpdateFavoriteFolders(UpdateFavoriteFoldersParameters parameters, CancellationToken ct = default(CancellationToken))
        {
            return await InvokeAsync<UpdateFavoriteFoldersResult, UpdateFavoriteFoldersParameters>("UpdateFavoriteFolders", parameters, ct);
        }

        public async Task<CancelSendDocumentResult> CancelSendDocumentAsync(CancelSendDocumentParameters parameters, CancellationToken ct = default)
        {
            return await InvokeAsync<CancelSendDocumentResult, CancelSendDocumentParameters>("CancelSendDocument", parameters, ct);
        }

        public async Task<ForceSendDocumentResult> ForceSendDocumentAsync(ForceSendDocumentParameters parameters, CancellationToken ct = default)
        {
            return await InvokeAsync<ForceSendDocumentResult, ForceSendDocumentParameters>("ForceSendDocument", parameters, ct);
        }

        public async Task<GetNewDocumentReferenceNumberResult> GetNewDocumentReferenceNumberAsync(GetNewDocumentReferenceNumberParameters parameters, CancellationToken ct = default)
        {
            return await InvokeAsync<GetNewDocumentReferenceNumberResult, GetNewDocumentReferenceNumberParameters>("GetNewDocumentReferenceNumber", parameters, ct);
        }
        
        public async Task<AddExtraFieldResult> AddExtraFieldAsync(AddExtraFieldParameters parameters, CancellationToken ct = default)
        {
            return await InvokeAsync<AddExtraFieldResult, AddExtraFieldParameters>("AddExtraField", parameters, ct);
        }

        public async Task<DeleteExtraFieldResult> DeleteExtraFieldAsync(DeleteExtraFieldParameters parameters, CancellationToken ct = default)
        {
            return await InvokeAsync<DeleteExtraFieldResult, DeleteExtraFieldParameters>("DeleteExtraField", parameters, ct);
        }

        public async Task<UpdateExtraFieldsResult> UpdateExtraFieldsAsync(UpdateExtraFieldsParameters parameters, CancellationToken ct = default)
        {
            return await InvokeAsync<UpdateExtraFieldsResult, UpdateExtraFieldsParameters>("UpdateExtraFields", parameters, ct);
        }

        public async Task<UpdateExtraFieldResult> UpdateExtraFieldAsync(UpdateExtraFieldParameters parameters, CancellationToken ct = default)
        {
            return await InvokeAsync<UpdateExtraFieldResult, UpdateExtraFieldParameters>("UpdateExtraField", parameters, ct);
        }

        public async Task<GetExtraFieldsResult> GetExtraFieldsAsync(GetExtraFieldsParameters parameters, CancellationToken ct = default)
        {
            return await InvokeAsync<GetExtraFieldsResult, GetExtraFieldsParameters>("GetExtraFields", parameters, ct);
        }

        public async Task<GetDocumentExtraFieldResult> GetDocumentExtraFieldAsync(GetDocumentExtraFieldParameters parameters, CancellationToken ct = default)
        {
            return await InvokeAsync<GetDocumentExtraFieldResult, GetDocumentExtraFieldParameters>("GetDocumentExtraField", parameters, ct);
        }

        public async Task<GetDocumentExtraFieldsResult> GetDocumentExtraFieldsAsync(GetDocumentExtraFieldsParameters parameters, CancellationToken ct = default)
        {
            return await InvokeAsync<GetDocumentExtraFieldsResult, GetDocumentExtraFieldsParameters>("GetDocumentExtraFields", parameters, ct);
        }

        public async Task<AssignDocumentExtraFieldResult> AssignDocumentExtraFieldAsync(AssignDocumentExtraFieldParameters parameters, CancellationToken ct = default)
        {
            return await InvokeAsync<AssignDocumentExtraFieldResult, AssignDocumentExtraFieldParameters>("AssignDocumentExtraField", parameters, ct);
        }

        public async Task<DeleteDocumentExtraFieldResult> DeleteDocumentExtraFieldAsync(DeleteDocumentExtraFieldParameters parameters, CancellationToken ct = default)
        {
            return await InvokeAsync<DeleteDocumentExtraFieldResult, DeleteDocumentExtraFieldParameters>("DeleteDocumentExtraField", parameters, ct);
        }

        public async Task<GetTransmitInfoResult> GetDocumentTransmitInfoAsync(GetTransmitInfoParameters parameters, CancellationToken ct = default)
        {
            return await InvokeAsync<GetTransmitInfoResult, GetTransmitInfoParameters>("GetTransmitInfo", parameters, ct);
        }

        public async Task<GetAutoReplyResult> GetAutoReplyRuleAsync(GetAutoReplyParameters parameters, CancellationToken ct = default)
        {
            return await InvokeAsync<GetAutoReplyResult, GetAutoReplyParameters>("GetAutoReply", parameters, ct);
        }

        public async Task<SetAutoReplyResult> SetAutoReplyRuleAsync(SetAutoReplyParameters parameters, CancellationToken ct = default)
        {
            return await InvokeAsync<SetAutoReplyResult, SetAutoReplyParameters>("SetAutoReply", parameters, ct);
        }

    }
}