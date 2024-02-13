using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mark5.Mobile.Classes.Azure;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Identity.Client;
using Microsoft.Kiota.Abstractions.Authentication;
using Newtonsoft.Json;
using Xamarin.Essentials;
using Constants = Mark5.Mobile.Classes.AuthService.MicrosoftGraphConstants;

namespace Mark5.Mobile.Classes.AuthService
{
    public class MicrosoftGraphClient: IMicrosoftGraphClient
    {
        readonly IPublicClientApplication pca;
        readonly GraphServiceClient graphClient;
        string accessToken;
        string directoryId;
        public static IAccount Account;

        public MicrosoftGraphClient()
        {
            try
            {
                if (DeviceInfo.Platform == DevicePlatform.iOS)
                {
                    pca = PublicClientApplicationBuilder.Create(Constants.ClientId)
                            .WithRedirectUri(Constants.IosRedirectUri)
                            .WithIosKeychainSecurityGroup("com.microsoft.adalcache")
                            .Build();

                }
                else if (DeviceInfo.Platform == DevicePlatform.Android)
                {
                    pca = PublicClientApplicationBuilder.Create(Constants.ClientId)
                            .WithRedirectUri(Constants.AndroidRedirectUri)
                            .Build();
                }

                var authenticationProvider = new BaseBearerTokenAuthenticationProvider(new TokenProvider(pca, Constants.Scopes()));
                graphClient = new GraphServiceClient(authenticationProvider);

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
                // this.account = account;
                this.scopes = scopes;
            }

            public async Task<string> GetAuthorizationTokenAsync(Uri uri, Dictionary<string, object> additionalAuthenticationContext = default,
                CancellationToken cancellationToken = default)
            {
                var result = await pca.AcquireTokenSilent(scopes, Account)
                            .ExecuteAsync();
                return result.AccessToken;
            }

            public AllowedHostsValidator AllowedHostsValidator { get; }
        }

        public async Task<string> Authenticate(object parentWindow, bool forceInteractive = true)
        {
            if (Account != null && string.IsNullOrEmpty(accessToken))
                return accessToken;

            AuthenticationResult authResult = null;
            var accounts = await pca.GetAccountsAsync();
            if (accounts.Count() > 1)
                forceInteractive = true;

            if (!forceInteractive)
            {
                try
                {
                    Account = accounts.FirstOrDefault();
                    authResult = await pca.AcquireTokenSilent(Constants.Scopes(), Account).ExecuteAsync();
                }
                catch (MsalUiRequiredException)
                {
                    //The login needs to be interactive, nothing to do
                }
            }

            if (Account == null)
            {
                // The user was not already connected.
                authResult = await pca.AcquireTokenInteractive(Constants.Scopes())
                                           .WithParentActivityOrWindow(parentWindow)
                                           .ExecuteAsync();
            }

            if (authResult != null)
            {
                accessToken = authResult.AccessToken;
                directoryId = authResult.TenantId;
                Account = authResult.Account;
            }
            return accessToken;
        }

        public async Task<AzureUser> GetAzureUser()
        {
            var user = await graphClient.Me.GetAsync();

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

            var extension = await graphClient.Organization[directoryId].Extensions[Constants.EndpointInfoExtName].GetAsync();
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
                var extension = await graphClient.Organization[directoryId].Extensions[Constants.AppProxyExtName].GetAsync();
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
            try
            {
                var page = await graphClient.Me.Events.GetAsync( (requestConfiguration) =>
{
                    requestConfiguration.QueryParameters.Filter = $"iCalUId eq '{iCalUid}'";
                });
                    
                return page.Value.FirstOrDefault();//.AsResult();
            }
            catch (Exception e)
            {
                return null;
                //return Result.Fail<Event>($"Unable to get event from MS Graph: {e}");
            }
        }

        private async Task AcceptEventAsync(string eventKey) 
            => await graphClient.Me.Events[eventKey].Accept.PostAsync(new Microsoft.Graph.Me.Events.Item.Accept.AcceptPostRequestBody());

        private async Task DeclineEventAsync(string eventKey)
            => await graphClient.Me.Calendar.Events[eventKey].Decline.PostAsync(new Microsoft.Graph.Me.Calendar.Events.Item.Decline.DeclinePostRequestBody());

        private async Task TentativelyAcceptEventAsync(string eventKey)
            => await graphClient.Me.Calendar.Events[eventKey].TentativelyAccept.PostAsync(new Microsoft.Graph.Me.Calendar.Events.Item.TentativelyAccept.TentativelyAcceptPostRequestBody());

        public async Task<Event> ImportFromICal((string Id, List<Common.Model.Attendee> Attendees) iEvent,
         List<string> participantAddressesToUpdate)
        {

            if (string.IsNullOrEmpty(iEvent.Id))
                return null;

            var result = await GetEventByICalUidAsync(iEvent.Id);

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
        }
    }

        #endregion
    }

