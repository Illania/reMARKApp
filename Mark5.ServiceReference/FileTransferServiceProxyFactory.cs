using System;
using System.Net.Http;
using Mark5.ServiceReference.FileTransferService;

namespace Mark5.ServiceReference
{
    public static class FileTransferServiceProxyFactory
    {
        public static IFileTransferServiceProxy Create(bool ssl, string hostname, int port, Func<HttpMessageHandler> httpClientHandler, Action onStartTransmission, Action onStopTransmission)
        {
            return new FileTransferServiceProxy(ssl, hostname, port, httpClientHandler, onStartTransmission, onStopTransmission);
        }
    }
}