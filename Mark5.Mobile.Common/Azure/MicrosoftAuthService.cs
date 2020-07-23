using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Model;
using Microsoft.Identity.Client;
using Newtonsoft.Json;

namespace Mark5.Mobile.Common.Azure
{
    public class MicrosoftAuthService
    {
        readonly string clientId = "ca4a3013-2f7f-4733-aa6c-126c8d34216f";
        readonly string redirectURI = "msauth.com.nordic-it.mark5.mobile.ios://auth"; //TODO it is different for Android
        readonly string[] scopes = { "User.Read" };

        private static readonly string graphApiUrl = "https://graph.microsoft.com/v1.0";
        private static readonly string graphCurrentUserUrl = $"{graphApiUrl}/me";

        readonly IPublicClientApplication pca;
        string accessToken;
        IAccount account;

        public MicrosoftAuthService()
        {
            pca = PublicClientApplicationBuilder.Create(clientId)
                                    .WithRedirectUri(redirectURI)
                                    .WithIosKeychainSecurityGroup("com.microsoft.adalcache")
                                    .Build();
        }

        public async Task Authenticate(object parentWindow, bool forceInteractive = false)
        {
            if (account != null && string.IsNullOrEmpty(accessToken))
                return;

            AuthenticationResult authResult = null;
            var accounts = await pca.GetAccountsAsync();

            if (!forceInteractive)
            {
                try
                {
                    account = accounts.FirstOrDefault();
                    authResult = await pca.AcquireTokenSilent(scopes, account).ExecuteAsync();
                }
                catch (MsalUiRequiredException)
                {
                    //The login needs to be interactive, nothing to do
                }
            }

            if (account == null)
            {
                // The user was not already connected.
                authResult = await pca.AcquireTokenInteractive(scopes)
                                           .WithParentActivityOrWindow(parentWindow)
                                           .ExecuteAsync();
            }

            if (authResult != null)
            {
                accessToken = authResult.AccessToken;
                account = authResult.Account;
            }
        }

        public async Task<AzureUser> GetAzureUser()
        {
            using var client = new HttpClient();
            var message = new HttpRequestMessage(HttpMethod.Get, graphCurrentUserUrl);
            message.Headers.Authorization = new AuthenticationHeaderValue("bearer", accessToken);
            HttpResponseMessage response = await client.SendAsync(message);
            AzureUser currentUser = null;

            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();
                currentUser = JsonConvert.DeserializeObject<AzureUser>(json);
            }

            return currentUser;
        }

        public async Task<List<AzureEndpointInfo>> GetAzureEndpointInfoList()
        {
            //TODO this function needs to be modified, this is only for testing
            var endpointInfo = new AzureEndpointInfo
            {
                Name = "test",
                Hostname = "hostname",
                Port = 8096,
                SslMode = SslMode.On
            };

            return new List<AzureEndpointInfo> { endpointInfo, endpointInfo };
        }
    }
}
