using System;
using System.Net.Http;
using reMark.Mobile.Classes.Azure;
using reMark.ServiceReference.FileTransferService;

namespace reMark.ServiceReference
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