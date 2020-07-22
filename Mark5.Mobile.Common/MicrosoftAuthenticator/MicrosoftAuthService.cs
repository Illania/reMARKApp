using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using Newtonsoft.Json;

namespace Mark5.Mobile.Common.MicrosoftAuthenticator
{
    public class MicrosoftAuthenticator
    {
        string ClientID = "ca4a3013-2f7f-4733-aa6c-126c8d34216f";
        string RedirectURI = "msauth.com.nordic-it.mark5.mobile.ios://auth"; //TODO it is different for Android
        string[] Scopes = { "User.Read" };

        IPublicClientApplication pca;
        string accessToken;
        IAccount account;

        private static string GraphApiUrl = "https://graph.microsoft.com/v1.0";
        private static string GraphCurrentUserUrl = $"{GraphApiUrl}/me";

        public MicrosoftAuthenticator()
        {
            pca = PublicClientApplicationBuilder.Create(ClientID)
                                    .WithRedirectUri(RedirectURI)
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
                    authResult = await pca.AcquireTokenSilent(Scopes, account).ExecuteAsync();
                }
                catch (MsalUiRequiredException)
                {
                    //The login needs to be interactive
                }
            }

            if (account == null)
            {
                // The user was not already connected.
                authResult = await pca.AcquireTokenInteractive(Scopes)
                                           .WithParentActivityOrWindow(parentWindow)
                                           .ExecuteAsync();
            }

            if (authResult != null)
            {
                accessToken = authResult.AccessToken;
                account = authResult.Account;
            }
        }

        public async Task<MicrosoftUser> GetCurrentUser()
        {
            using var client = new HttpClient();
            var message = new HttpRequestMessage(HttpMethod.Get, GraphCurrentUserUrl);
            message.Headers.Authorization = new AuthenticationHeaderValue("bearer", accessToken);
            HttpResponseMessage response = await client.SendAsync(message);
            MicrosoftUser currentUser = null;

            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();
                currentUser = JsonConvert.DeserializeObject<MicrosoftUser>(json);
            }

            return currentUser;
        }

        public class MicrosoftUser
        {
            public string Id { get; set; }
            public string DisplayName { get; set; }
            public string Mail { get; set; }
        }

    }
}
