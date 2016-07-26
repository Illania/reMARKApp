//
// Project: Mark5.Mobile.ServiceReference
// File: FileTransferServiceProxyFactory.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Mark5.ServiceReference.FileTransferService;

namespace Mark5.ServiceReference
{

    public static class FileTransferServiceProxyFactory
    {

        public static IFileTransferServiceProxy Create(bool ssl, string hostname, int port)
        {
            return new FileTransferServiceProxy(ssl, hostname, port);
        }
    }
}

