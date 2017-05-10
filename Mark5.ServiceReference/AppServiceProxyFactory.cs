//
// Project: Mark5.Mobile.ServiceReference
// File: AppServiceProxyFactory.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Net.Http;
using Mark5.ServiceReference.AppService;

#pragma warning disable CS1701
namespace Mark5.ServiceReference
{

    public static class AppServiceProxyFactory
    {

        public static IAppServiceProxy Create(bool ssl, string hostname, int port, Func<HttpMessageHandler> httpClientHandler)
        {
#if DEBUG
            return new HttpAppServiceProxy(ssl, hostname, port, httpClientHandler);
#else
            return new WcfAppServiceProxy(ssl, hostname, port);
#endif
        }
    }
}

