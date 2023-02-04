using System;
using System.Net.Http;
using Mark5.Mobile.Classes;
using Mark5.Mobile.Classes.Azure;
using Mark5.ServiceReference.AppService;

namespace Mark5.ServiceReference
{
    public static class AppServiceProxyFactory
    {
        public static IAppServiceProxy Create(bool ssl, string hostname, string port, Func<HttpMessageHandler> httpClientHandler,
            Action onStartTransmission, Action onStopTransmission,
            IReachability reachability,
            string bearer = "",
            AzureApplicationProxyInfo azureApplicationProxyInfo = null)
        {
            return new HttpAppServiceProxy(ssl, hostname, port, httpClientHandler, onStartTransmission, onStopTransmission,
                reachability, bearer, azureApplicationProxyInfo);
        }
    }
}