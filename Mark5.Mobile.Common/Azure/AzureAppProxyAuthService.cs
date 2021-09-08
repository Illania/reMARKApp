using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using Xamarin.Essentials;

namespace Mark5.Mobile.Common.Azure
{
    public class AzureAppProxyAuthService
    {
        const string iosRedirectURI = "msauth.com.nordic-it.mark5.mobile.ios://auth";
        const string androidRedirectURI = "msauth://com.nordic_it.mark5.android/dUOzGWwhv%2BzH%2F6bxqKb4ZlnNC8M%3D";
        readonly List<string> scopes = new List<string> { "User.Read" };
        readonly IPublicClientApplication pca;
        string accessToken;
        IAccount account;

        public AzureAppProxyAuthService(string appClientId, string proxyClientId)
        {
  
            scopes.Add($"{proxyClientId}/user_impersonation");

            try
            {
                if (DeviceInfo.Platform == DevicePlatform.iOS)
                {
                    pca = PublicClientApplicationBuilder.Create(appClientId)
                            .WithRedirectUri(iosRedirectURI)
                            .WithIosKeychainSecurityGroup("com.microsoft.adalcache")
                            .Build();

                }
                else if (DeviceInfo.Platform == DevicePlatform.Android)
                {
                    pca = PublicClientApplicationBuilder.Create(appClientId)
                            .WithRedirectUri(androidRedirectURI)
                            .Build();
                }
            }
            catch (Exception ex)
            {
                var message = ex.Message;
            }

        }

        public async Task<string> Authenticate(object parentWindow, bool forceInteractive = true)
        {
            if (account != null && string.IsNullOrEmpty(accessToken))
                return accessToken;

            AuthenticationResult authResult = null;
            var accounts = await pca.GetAccountsAsync();
            if (accounts.Count() > 1)
                forceInteractive = true;

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
            return accessToken;
        }

    }
}






     
       
