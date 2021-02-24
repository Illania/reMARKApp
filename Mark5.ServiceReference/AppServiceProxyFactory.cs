using System;
using System.Net.Http;
using Mark5.ServiceReference.AppService;

namespace Mark5.ServiceReference
{
    public static class AppServiceProxyFactory
    {
        public static IAppServiceProxy Create(bool ssl, string hostname, string port, Func<HttpMessageHandler> httpClientHandler, Action onStartTransmission, Action onStopTransmission)
        {
            return new HttpAppServiceProxy(ssl, hostname, port, httpClientHandler, onStartTransmission, onStopTransmission);
        }
    }
}