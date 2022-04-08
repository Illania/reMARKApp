using System;
using System.Net.Http;
using Mark5.Mobile.Classes.Azure;
using Mark5.ServiceReference.FileTransferService;

namespace Mark5.ServiceReference
{
    public static class FileTransferServiceProxyFactory
    {
        public static IFileTransferServiceProxy Create(bool ssl, string hostname, string port,
            Func<HttpMessageHandler> httpClientHandler, Action onStartTransmission, Action onStopTransmission,
            string bearerToken, AzureApplicationProxyInfo azureApplicationProxyInfo)
        {
            return new FileTransferServiceProxy(ssl, hostname, port, httpClientHandler, onStartTransmission, onStopTransmission,
                bearerToken, azureApplicationProxyInfo);
        }
    }
}