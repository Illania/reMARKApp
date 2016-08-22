//
// Project: Mark5.Mobile.Droid
// File: Mark5Application.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Runtime;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Database;
using Mark5.Mobile.Droid.Services;
using Mark5.Mobile.Droid.Utilities;
using PCLStorage;
using Xamarin;

namespace Mark5.Mobile.Droid
{
    [Application]
    public class Mark5Application : Application
    {

        public Mark5Application(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
            int workerThreads, completionPortThreads;
            ThreadPool.GetMinThreads(out workerThreads, out completionPortThreads);
            ThreadPool.SetMinThreads(workerThreads * 10, completionPortThreads * 10);
        }

        public override void OnCreate()
        {
            base.OnCreate();

#if RELEASE
            Insights.Initialize("9797448a2139873ddf4487f52d80128bbbf8933a", Context, true);
#else
            Insights.Initialize(Insights.DebugModeKey, Context, true);
#endif

            Insights.Track($"[{nameof(Mark5Application.OnCreate)}] Creating application...");

            Task.Run(async () =>
            {
                var mainFolder = FileSystem.Current.LocalStorage;
                var dataFolder = await mainFolder.CreateFolderAsync(PortablePath.Combine("data", "data"), CreationCollisionOption.OpenIfExists);
                var cacheFolder = await mainFolder.CreateFolderAsync(PortablePath.Combine("..", "cache"), CreationCollisionOption.OpenIfExists);
                var dbFolder = await mainFolder.CreateFolderAsync(PortablePath.Combine("data", "db"), CreationCollisionOption.OpenIfExists);
                var attachmentsFolder = await mainFolder.CreateFolderAsync(PortablePath.Combine("data", "att"), CreationCollisionOption.OpenIfExists);

                CommonConfig.DataFolder = dataFolder;
                CommonConfig.CacheFolder = cacheFolder;
                CommonConfig.DatabaseFolder = dbFolder;
                CommonConfig.AttachmentsFolder = attachmentsFolder;
                CommonConfig.ReachabilityService = new ReachabilityService();

                await DatabaseUtils.InitializeDatabases();

                PlatformConfig.SSLCertificateVerificationManager = new SSLCertificateVerificationManager();
                PlatformConfig.ReachabilityBroadcastReceiver = new ReachabilityBroadcastReceiver();
            }).Wait();

            Insights.Track($"[{nameof(Mark5Application.OnCreate)}] Created created application.");
        }
    }
}

