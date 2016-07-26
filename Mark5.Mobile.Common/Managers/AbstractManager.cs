//
// Project: Mark5.Mobile.Common
// File: AbstractManager.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Mark5.Mobile.Common.Model;
using Mark5.ServiceReference.AppService;

namespace Mark5.Mobile.Common.Managers
{

    abstract class AbstractManager
    {

        protected string Token
        {
            get
            {
                return ConnectionInfo.Token;
            }
        }

        protected readonly ConnectionInfo ConnectionInfo;
        protected readonly IAppServiceProxy AppServiceProxy;

        protected AbstractManager(ConnectionInfo connectionInfo, IAppServiceProxy appServiceProxy)
        {
            ConnectionInfo = connectionInfo;
            AppServiceProxy = appServiceProxy;
        }
    }
}

