using System;
using System.Net.Http;
using reMark.Mobile.Classes;
using reMark.Mobile.Classes.Azure;
using reMark.ServiceReference.AppService;

namespace reMark.ServiceReference
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