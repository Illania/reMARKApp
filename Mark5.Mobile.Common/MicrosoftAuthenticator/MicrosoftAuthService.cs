using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Identity.Client;

namespace Mark5.Mobile.Common.MicrosoftAuthenticator
{
    public class MicrosoftAuthService
    {
        string ClientID = "ca4a3013-2f7f-4733-aa6c-126c8d34216f";
        string RedirectURI = "msauth.com.nordic-it.mark5.mobile.ios://auth"; //TODO it is different for Android
        string[] Scopes = { "User.Read" };

        IPublicClientApplication pca;

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

                    var firstAccount = accounts.FirstOrDefault();
                    var authResult = await pca.AcquireTokenSilent(Scopes, firstAccount).ExecuteAsync();
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
                            token = authResult.AccessToken;
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
    }
}
