using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Model.Azure;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using Xamarin.Essentials;

namespace Mark5.Mobile.Common.Azure
{
    public class MicrosoftAuthService
    {
        const string clientId = "ca4a3013-2f7f-4733-aa6c-126c8d34216f";
        const string iosRedirectURI = "msauth.com.nordic-it.mark5.mobile.ios://auth";
        const string androidRedirectURI = "msauth://com.nordic_it.mark5.android/dUOzGWwhv%2BzH%2F6bxqKb4ZlnNC8M%3D";
        const string endpointInfoExtName = "com.remark-app.endpoint";
        const string appProxyExtName = "com.remark-app.proxy";
        readonly string[] scopes = { "User.Read" };

        readonly IPublicClientApplication pca;
        readonly GraphServiceClient graphClient;
        string accessToken;
        string directoryId;
        IAccount account;

        public MicrosoftAuthService()
        {
            try
            {
                if (DeviceInfo.Platform == DevicePlatform.iOS)
                {
                    pca = PublicClientApplicationBuilder.Create(clientId)
                            .WithRedirectUri(iosRedirectURI)
                            .WithIosKeychainSecurityGroup("com.microsoft.adalcache")
                            .Build();

                }
                else if (DeviceInfo.Platform == DevicePlatform.Android)
                {
                    pca = PublicClientApplicationBuilder.Create(clientId)
                            .WithRedirectUri(androidRedirectURI)
                            .Build();
                }

                graphClient = new GraphServiceClient(new DelegateAuthenticationProvider(
                    async (requestMessage) =>
                    {
                        var result = await pca.AcquireTokenSilent(scopes, account)
                            .ExecuteAsync();

                        requestMessage.Headers.Authorization =
                            new AuthenticationHeaderValue("Bearer", result.AccessToken);
                    }));
            }
            catch(Exception ex)
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
                directoryId = authResult.TenantId;
                account = authResult.Account;
            }
            return accessToken;
        }

        public async Task<AzureUser> GetAzureUser()
        {
            var user = await graphClient.Me
                   .Request()
                   .GetAsync();

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

            var extension = await graphClient.Organization[directoryId].Extensions[endpointInfoExtName].Request().GetAsync();
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
                var extension = await graphClient.Organization[directoryId].Extensions[appProxyExtName].Request().GetAsync();
                addData = extension.AdditionalData;
            }
            catch(Exception ex)
            {
                addData = null;
            }
            

            if(addData !=  null)
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


    }
}
