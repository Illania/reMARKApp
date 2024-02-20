using reMark.Mobile.Classes.Azure;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Identity.Client;
using Microsoft.Kiota.Abstractions.Authentication;
using Newtonsoft.Json;
using Constants = reMark.Mobile.Common.Manager.MicrosoftGraphConstants;
using Microsoft.Maui.Platform;
using reMark.Mobile.Common.Model.Exceptions;

namespace reMark.Mobile.Common.Manager
{ 
    public class MicrosoftGraphClient: IMicrosoftGraphClient
    {
        private readonly IPublicClientApplication _pca;
        private readonly GraphServiceClient _graphClient;
        private string _directoryId;
        private static IAccount _account;
        public string AccessToken {get; set;}
        
  
        public MicrosoftGraphClient()
        {
            try
            {
                if (DeviceInfo.Platform == DevicePlatform.iOS)
                {
                    _pca = PublicClientApplicationBuilder.Create(Constants.ClientId)
                            .WithRedirectUri(Constants.IosRedirectUri)
                            .WithIosKeychainSecurityGroup("com.microsoft.adalcache")
                            .Build();

                }
                else if (DeviceInfo.Platform == DevicePlatform.Android)
                {
                    _pca = PublicClientApplicationBuilder.Create(Constants.ClientId)
                            .WithRedirectUri(Constants.AndroidRedirectUri)
                            .Build();
                }

                var authenticationProvider = new BaseBearerTokenAuthenticationProvider(new TokenProvider(_pca, Constants.Scopes()));
                _graphClient = new GraphServiceClient(authenticationProvider);

            }
            catch (Exception ex)
            {
                var message = ex.Message;
            }

        }

        internal class TokenProvider : IAccessTokenProvider
        {
            readonly IPublicClientApplication pca;
            readonly string[] scopes;

            public TokenProvider(IPublicClientApplication pca, string[] scopes)
            {
                this.pca = pca;
                this.scopes = scopes;
            }

            public async Task<string> GetAuthorizationTokenAsync(Uri uri, Dictionary<string, object> additionalAuthenticationContext = default,
                CancellationToken cancellationToken = default)
            {
                var result = await pca.AcquireTokenSilent(scopes, _account)
                            .ExecuteAsync();
                return result.AccessToken;
            }

            public AllowedHostsValidator AllowedHostsValidator { get; }
        }

        public async Task<string> Authenticate(object parentWindow, bool forceInteractive = true)
        {
            if (_account != null && string.IsNullOrEmpty(AccessToken))
                return AccessToken;

            AuthenticationResult authResult = null;
            var accounts = await _pca.GetAccountsAsync();
            if (accounts.Count() > 1)
                forceInteractive = true;

            if (!forceInteractive)
            {
                try
                {
                    _account = accounts.FirstOrDefault();
                    authResult = await _pca.AcquireTokenSilent(Constants.Scopes(), _account).ExecuteAsync();
                }
                catch (MsalUiRequiredException)
                {
                    //The login needs to be interactive, nothing to do
                }
            }

            if (_account == null)
            {
                // The user was not already connected.
                authResult = await _pca.AcquireTokenInteractive(Constants.Scopes())
                                        .WithParentActivityOrWindow(parentWindow)
                                        .ExecuteAsync();
            }

            if (authResult != null)
            {
                AccessToken = authResult.AccessToken;
                _directoryId = authResult.TenantId;
                _account = authResult.Account;
            }
            return AccessToken;
        }

        public bool IsAuthenticated() => string.IsNullOrEmpty(AccessToken);

        public async Task<AzureUser> GetAzureUser()
        {
            var user = await _graphClient.Me.GetAsync();

            return new AzureUser
            {
                Id = user.Id,
                UserPrincipalName = user.UserPrincipalName,
                DisplayName = user.DisplayName,
                Mail = user.Mail,
            };
        }

        public async Task<List<AzureEndpointInfo>> GetAzureEndpointInfoList()
        {
            var endpointList = new List<AzureEndpointInfo>();

            var extension = await _graphClient.Organization[_directoryId].Extensions[Constants.EndpointInfoExtName].GetAsync();
            var addData = extension.AdditionalData;

            foreach (var key in addData.Keys)
            {
                try
                {
                    var info = JsonConvert.DeserializeObject<AzureEndpointInfo>(addData[key].ToString());
                    if (!string.IsNullOrEmpty(info.Name) && !string.IsNullOrEmpty(info.Hostname))
                        endpointList.Add(info);
                }
                catch (JsonReaderException)
                {
                    //We expect this kind of exception
                    //If this happens it means we are trying to parse one of the "default" elements of extension additional data
                }
            }

            return endpointList;
        }

        public async Task<AzureApplicationProxyInfo> GetAzureApplicationProxyInfo()
        {
            var proxyInfo = new AzureApplicationProxyInfo();
            IDictionary<string, object> addData;
            try
            {
                var extension = await _graphClient.Organization[_directoryId].Extensions[Constants.AppProxyExtName].GetAsync();
                addData = extension.AdditionalData;
            }
            catch (Exception ex)
            {
                addData = null;
            }


            if (addData != null)
            {
                try
                {
                    proxyInfo = new AzureApplicationProxyInfo()
                    {
                        IsEnabled = Convert.ToBoolean(addData["IsEnabled"]),
                        AppClientId = Convert.ToString(addData["AppClientId"]),
                        ApplicationProxyClientId = Convert.ToString(addData["ApplicationProxyClientId"])
                    };

                }
                catch (Exception ex)
                {
                }
            }

            return proxyInfo;
        }

        #region Calendar
        private async Task<Event> GetEventByICalUidAsync(string iCalUid)
        {
            var page = await _graphClient.Me.Events.GetAsync( (requestConfiguration) =>
            {
                requestConfiguration.QueryParameters.Filter = $"iCalUId eq '{iCalUid}'";
            });
                
            return page.Value.FirstOrDefault();
        }

        private async Task AcceptEventAsync(string eventKey) 
            => await _graphClient.Me.Events[eventKey].Accept.PostAsync(new Microsoft.Graph.Me.Events.Item.Accept.AcceptPostRequestBody());

        private async Task DeclineEventAsync(string eventKey)
            => await _graphClient.Me.Calendar.Events[eventKey].Decline.PostAsync(new Microsoft.Graph.Me.Calendar.Events.Item.Decline.DeclinePostRequestBody());

        private async Task TentativelyAcceptEventAsync(string eventKey)
            => await _graphClient.Me.Calendar.Events[eventKey].TentativelyAccept.PostAsync(new Microsoft.Graph.Me.Calendar.Events.Item.TentativelyAccept.TentativelyAcceptPostRequestBody());

        public async Task<Event> ImportFromICal((string Id, List<Common.Model.Attendee> Attendees) iEvent,
         List<string> participantAddressesToUpdate)
        {

            if (string.IsNullOrEmpty(iEvent.Id))
                return null;

            var result = await GetEventFromMsGraph(iEvent);

            if (result == null)
                return null;

            var calendarEvent = result;
            var attendee = iEvent.Attendees
                .FirstOrDefault(att
                    => participantAddressesToUpdate.Any(participant
                        => att.Name.Equals(participant, StringComparison.InvariantCultureIgnoreCase)));

            calendarEvent.AdditionalData["ExternalIcsUid"] = iEvent.Id;

            switch (attendee?.Status)
            {
                case Common.Model.ParticipantStatus.Accepted:
                    await AcceptEventAsync(calendarEvent.Id);
                    calendarEvent.ShowAs = FreeBusyStatus.Busy;
                    break;
                case Common.Model.ParticipantStatus.Declined:
                    await DeclineEventAsync(calendarEvent.Id);

                    break;
                case Common.Model.ParticipantStatus.Tentative:
                    await TentativelyAcceptEventAsync(calendarEvent.Id);
                    calendarEvent.ShowAs = FreeBusyStatus.Tentative;
                    break;
            }

            ResponseType GetResponseStatus(Common.Model.ParticipantStatus status)
            {
                switch (status)
                {
                    case Common.Model.ParticipantStatus.Accepted:
                        return ResponseType.Accepted;
                    case Common.Model.ParticipantStatus.Declined:
                        return ResponseType.Declined;
                    case Common.Model.ParticipantStatus.Tentative:
                        return ResponseType.TentativelyAccepted;
                    default:
                        return ResponseType.NotResponded;
                }
            }

            if (attendee == null)
                return calendarEvent;

            var calendarAttendee = calendarEvent.Attendees.FirstOrDefault(att =>
                att.EmailAddress.Address.Equals(attendee.Name, StringComparison.InvariantCultureIgnoreCase));

            if (calendarAttendee != null)
            {
                calendarAttendee.Status = new ResponseStatus
                {
                    Response = GetResponseStatus(attendee.Status)
                };
            }
            return calendarEvent;

            async Task<Event> GetEventFromMsGraph((string Id, List<Model.Attendee> Attendees) iEvent)
            {
                try
                {
                    return await GetEventByICalUidAsync(iEvent.Id);
                }
                catch(Exception ex)
                {
                    throw new ReMarkException(ErrorConstants.Codes.CantGetEventFromMsGraph);
                }
                
            }
        }
    }

        #endregion
    }

