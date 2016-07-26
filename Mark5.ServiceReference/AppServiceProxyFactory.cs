//
// Project: Mark5.Mobile.ServiceReference
// File: AppServiceProxyFactory.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Mark5.ServiceReference.AppService;

namespace Mark5.ServiceReference
{

    public static class AppServiceProxyFactory
    {

        public static IAppServiceProxy Create(bool ssl, string hostname, int port)
        {
            return new AppServiceProxy(ssl, hostname, port);
        }
    }
}

