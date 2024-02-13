using reMark.Mobile.Classes.Azure;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using Microsoft.Kiota.Abstractions.Authentication;

namespace reMark.Mobile.Classes.AuthService
{
    public class MicrosoftAuthService
    {
        const string clientId = "ca4a3013-2f7f-4733-aa6c-126c8d34216f";
        const string iosRedirectURI = "msauth.com.nordic-it.mark5.Mobile.IOS://auth";
        const string androidRedirectURI = "msauth://com.nordic_it.mark5.android/dUOzGWwhv%2BzH%2F6bxqKb4ZlnNC8M%3D";
        const string endpointInfoExtName = "com.remark-app.endpoint";
        const string appProxyExtName = "com.remark-app.proxy";
        

        readonly IPublicClientApplication pca;
        readonly GraphServiceClient graphClient;
        string accessToken;
        string directoryId;
        public static IAccount Account;
        readonly string[] scopes = { "User.Read" };

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

                var authenticationProvider = new BaseBearerTokenAuthenticationProvider(new TokenProvider(pca, scopes));
                graphClient = new GraphServiceClient(authenticationProvider);
            }
            catch(Exception ex)
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
                    authResult = await pca.AcquireTokenSilent(scopes, Account).ExecuteAsync();
                }
                catch (MsalUiRequiredException)
                {
                    //The login needs to be interactive, nothing to do
                }
            }

            if (Account == null)
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

            var extension = await graphClient.Organization[directoryId].Extensions[endpointInfoExtName].GetAsync();
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
                var extension = await graphClient.Organization[directoryId].Extensions[appProxyExtName].GetAsync();
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
                        IsEnabled = Convert.ToBoolean(addData["IsEnabled"].ToString().ToLower()),
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
