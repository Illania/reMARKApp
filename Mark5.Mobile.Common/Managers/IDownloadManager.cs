//
// Project: Mark5.Mobile.Common
// File: IDownloadManager.cs
// Author: Ferdinando Papale <fp@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System.Collections.Generic;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Common
{

    public interface IDownloadManager
    {

        Dictionary<ObjectType, DownloadPolicy> DownloadPolicies { get; }

        void Notify(ObjectType objectType, int folderId);

        Task<bool> IsRunning();

        Task Start();

        Task Stop();
    }
}

