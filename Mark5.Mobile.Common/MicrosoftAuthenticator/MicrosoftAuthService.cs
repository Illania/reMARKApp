using System;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Graph;
using Microsoft.Identity.Client;

namespace Mark5.Mobile.Common.MicrosoftAuthenticator
{
    public class MicrosoftAuthService
    {
        string ClientID = "ca4a3013-2f7f-4733-aa6c-126c8d34216f";
        string RedirectURI = "msauth.com.nordic-it.mark5.mobile.ios://auth"; //TODO it is different for Android
        string[] Scopes = { "User.Read" };

        GraphServiceClient GraphClient;
        IPublicClientApplication pca;
        IAccount account;

        public MicrosoftAuthService()
        {
            pca = PublicClientApplicationBuilder.Create(ClientID)
                                    .WithRedirectUri(RedirectURI)
                                    .WithIosKeychainSecurityGroup("com.microsoft.adalcache")
                                    .Build();
        }

        public async Task DoAuthentication(object parentWindow)
        {
            var token = string.Empty;
            var accounts = await pca.GetAccountsAsync();
            try
            {
                try
                {

                    account = accounts.FirstOrDefault();
                    var authResult = await pca.AcquireTokenSilent(Scopes, account).ExecuteAsync();
                    token = authResult.AccessToken;
                }
                catch (MsalUiRequiredException)
                {
                    // The user was not already connected.
                    try
                    {
                        var authResult = await this.pca.AcquireTokenInteractive(Scopes)
                                                    .WithParentActivityOrWindow(parentWindow)
                                                    .ExecuteAsync();

                        if (authResult != null)
                        {
                            token = authResult.AccessToken;
                            account = authResult.Account;
                        }
                    }
                    catch (Exception)
                    {
                        //TODO need to do something here
                    }
                }
            }
            catch (Exception)
            {
                //TODO need to do something here
            }
            //return token;

        }

        private async Task InitializeGraphClientAsync()
        {
            try
            {
                // Initialize Graph client
                GraphClient = new GraphServiceClient(new DelegateAuthenticationProvider(
                    async (requestMessage) =>
                    {
                        var result = await pca.AcquireTokenSilent(Scopes, account)
                            .ExecuteAsync();

                        requestMessage.Headers.Authorization =
                            new AuthenticationHeaderValue("Bearer", result.AccessToken);
                    }));
            }
            catch (Exception ex)
            {
            }
        }
    }
}
