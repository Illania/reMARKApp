//
// Project: Mark5.Mobile.Droid
// File: Mark5Application.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Runtime;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Database;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Services;
using Mark5.Mobile.Droid.Utilities;
using PCLStorage;
using TinyMessenger;
using Xamarin.Android.Net;

namespace Mark5.Mobile.Droid
{

    [Application(Theme = "@style/mark5")]
    public class Mark5Application : Application
    {

        public Mark5Application(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
            ThreadPool.SetMinThreads(50, 50);
            ThreadPool.SetMaxThreads(100, 100);
        }

        public override void OnCreate()
        {
            base.OnCreate();

            Task.Run(async () =>
            {
                var mainFolder = FileSystem.Current.LocalStorage;

                CommonConfig.PathSeparator = Path.DirectorySeparatorChar;
                CommonConfig.DataFolder = await mainFolder.CreateFolderAsync(PortablePath.Combine("data", "data"), CreationCollisionOption.OpenIfExists); ;
                CommonConfig.OutgoingFolder = await mainFolder.CreateFolderAsync(PortablePath.Combine("data", "out"), CreationCollisionOption.OpenIfExists); ;
                CommonConfig.DatabaseFolder = await mainFolder.CreateFolderAsync(PortablePath.Combine("data", "db"), CreationCollisionOption.OpenIfExists); ;
                CommonConfig.AttachmentsFolder = await mainFolder.CreateFolderAsync(PortablePath.Combine("..", "cache", "att"), CreationCollisionOption.OpenIfExists); ;
                CommonConfig.Logger = new SimpleLogger();
                CommonConfig.ReachabilityService = new ReachabilityService();
                CommonConfig.DeviceInfoProvider = new DeviceInfoProvider();
                CommonConfig.ConcurrentQueueType = typeof(PortableConcurrentQueue<>);
                CommonConfig.HttpClientHandler = () => { return new AndroidClientHandler(); };

#if !DEBUG
                CommonConfig.Logger.Level = LogLevel.INFO;
#else
                CommonConfig.Logger.Level = LogLevel.DEBUG;
#endif

                await DatabaseUtils.InitializeDatabases();

                PlatformConfig.SSLCertificateVerificationManager = new SSLCertificateVerificationManager();
                PlatformConfig.ReachabilityBroadcastReceiver = new ReachabilityBroadcastReceiver();
                PlatformConfig.Preferences = new Preferences();
                PlatformConfig.MessengerHub = new TinyMessengerHub();
            }).Wait();

            CommonConfig.Logger.Info($"Initialized {nameof(Mark5Application)}");
        }

        public override void OnTerminate()
        {
            base.OnTerminate();

            CommonConfig.Logger.Info($"Terminated {nameof(Mark5Application)}");
        }
    }
}

