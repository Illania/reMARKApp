//
// Project: Mark5.Mobile.IOS
// File: AppDelegate.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Database;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Utilities;
using Mark5.Mobile.IOS.Services;
using Mark5.Mobile.IOS.Utilities;
using PCLStorage;
using UIKit;

namespace Mark5.Mobile.IOS
{
    
    [Register("AppDelegate")]
    public class AppDelegate : UIApplicationDelegate
    {

        public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
            Task.Run(async () =>
            {
                var mainFolder = FileSystem.Current.LocalStorage;

                CommonConfig.PathSeparator = Path.DirectorySeparatorChar;
                CommonConfig.DataFolder = await mainFolder.CreateFolderAsync(PortablePath.Combine("data", "data"), CreationCollisionOption.OpenIfExists);
                CommonConfig.OutgoingFolder = await mainFolder.CreateFolderAsync(PortablePath.Combine("data", "out"), CreationCollisionOption.OpenIfExists);
                CommonConfig.DatabaseFolder = await mainFolder.CreateFolderAsync(PortablePath.Combine("data", "db"), CreationCollisionOption.OpenIfExists);
                CommonConfig.AttachmentsFolder = await mainFolder.CreateFolderAsync(PortablePath.Combine("..", "cache", "att"), CreationCollisionOption.OpenIfExists);
                CommonConfig.Logger = new SimpleLogger();
                CommonConfig.ReachabilityService = new ReachabilityService();
                CommonConfig.DeviceInfoProvider = new DeviceInfoProvider();
                CommonConfig.ConcurrentQueueType = typeof(PortableConcurrentQueue<>);
                CommonConfig.HttpClientHandler = () => { return new NSUrlSessionHandler(); };
                CommonConfig.PhonebookUtilities = new PhonebookUtilities();

#if !DEBUG
                CommonConfig.Logger.Level = LogLevel.INFO;
#else
                CommonConfig.Logger.Level = LogLevel.DEBUG;
#endif

                await DatabaseUtils.InitializeDatabases();

                PlatformConfig.SSLCertificateVerificationManager = new SSLCertificateVerificationManager();
                PlatformConfig.Preferences = new Preferences();
            }).Wait();

            return true;
        }
    }
}

