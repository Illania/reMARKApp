//
// Project: Mark5.Mobile.IOS
// File: AppDelegate.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Foundation;
using HockeyApp.iOS;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Authenticator;
using Mark5.Mobile.Common.Database;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Storage;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Utilities;
using Mark5.Mobile.IOS.Model.HubMessages;
using Mark5.Mobile.IOS.Services;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.ViewControllers;
using Mark5.Mobile.IOS.Utilities;
using PCLStorage;
using TinyMessenger;
using UIKit;
using UserNotifications;

namespace Mark5.Mobile.IOS
{

    [Register("AppDelegate")]
    public class AppDelegate : UIApplicationDelegate, IUNUserNotificationCenterDelegate
    {

        public override UIWindow Window
        {
            get;
            set;
        }

        public override bool WillFinishLaunching(UIApplication application, NSDictionary launchOptions)
        {
            try
            {
                var startupTime = new Stopwatch();
                startupTime.Start();

                InitializeCommon();

                CommonConfig.Logger.Info("MARK5 initializing...");
                var isLoggedIn = InitializePlatform(application);
                CommonConfig.Logger.Info("MARK5 initialized");

                BITHockeyManager.SharedHockeyManager.Configure(PlatformConfig.HockeyId);
                BITHockeyManager.SharedHockeyManager.CrashManager.CrashManagerStatus = PlatformConfig.Preferences.EnableReporting ? BITCrashManagerStatus.AutoSend : BITCrashManagerStatus.Disabled;
                BITHockeyManager.SharedHockeyManager.StartManager();
                BITHockeyManager.SharedHockeyManager.Authenticator.AuthenticateInstallation();

                Window = new UIWindow(UIScreen.MainScreen.Bounds);
                Theme.ApplyTheme(Window);

                UIViewController vc;
                if (!isLoggedIn)
                    vc = new LoginViewController();
                else if (Integration.IsIPad())
                    vc = new SplitMainViewController();
                else
                    vc = new SimpleMainViewController();

                Window.RootViewController = vc;
                Window.MakeKeyAndVisible();

                startupTime.Stop();
                CommonConfig.Logger.Info($"Total startup time: {startupTime.ElapsedMilliseconds}ms");

                return true;
            }
            catch (Exception ex)
            {
                CommonConfig.Logger?.Error(ex);

                return false;
            }
        }

        public override void ReceiveMemoryWarning(UIApplication application)
        {
            base.ReceiveMemoryWarning(application);

            CommonConfig.Logger.Warning("Received memery warning!");

            GC.Collect();
        }

        public override void RegisteredForRemoteNotifications(UIApplication application, NSData deviceToken)
        {
            CommonConfig.Logger.Info($"Received APNS token: {deviceToken}");

            var newToken = new string(deviceToken.ToString().Where(char.IsLetterOrDigit).ToArray());
            var oldToken = PlatformConfig.Preferences.PushNotificationToken;
            PlatformConfig.Preferences.PushNotificationToken = newToken;

            if (!string.IsNullOrWhiteSpace(oldToken) && oldToken != newToken)
            {
                CommonConfig.Logger.Info($"New APNS token is different, so try to unsubscribe old one...");
                Managers.NotificationsManager.UnSubscribe(DeviceType.IOS, oldToken).FireAndForget();
            }

            CommonConfig.Logger.Info($"Sending new APNS token...");
            Managers.NotificationsManager.Subscribe(DeviceType.IOS, newToken).FireAndForget();
        }

        public override void FailedToRegisterForRemoteNotifications(UIApplication application, NSError error)
        {
            CommonConfig.Logger.Error("Failed to received APNS Token", new NSErrorException(error));
            PlatformConfig.Preferences.PushNotificationToken = string.Empty;
        }

        [Export("userNotificationCenter:willPresentNotification:withCompletionHandler:")]
        public void WillPresentNotification(UNUserNotificationCenter center, UNNotification notification, Action<UNNotificationPresentationOptions> options)
        {
            if (notification.Request.Identifier != LocalNotificationsListener.FailedSendingIdentifier)
            {
                PlatformConfig.MessengerHub.PublishAsync(new NewNotificationsMessage(this));
            }

            options(UNNotificationPresentationOptions.Alert);
        }

        [Export("userNotificationCenter:didReceiveNotificationResponse:withCompletionHandler:")]
        public void DidReceiveNotificationResponse(UNUserNotificationCenter center, UNNotificationResponse response, Action completionHandler)
        {
            // TODO open document view

            completionHandler();
        }

        void InitializeCommon()
        {
            Task.Run(async () =>
            {
                var mainFolder = FileSystem.Current.LocalStorage;

                var preferences = new Preferences();

                if (preferences.ResetOnLaunch)
                    Integration.ClearData();

                CommonConfig.PathSeparator = Path.DirectorySeparatorChar;
                CommonConfig.DataFolder = await mainFolder.CreateFolderAsync(PortablePath.Combine("v2", "data"), CreationCollisionOption.OpenIfExists);
                CommonConfig.OutgoingFolder = await mainFolder.CreateFolderAsync(PortablePath.Combine("v2", "out"), CreationCollisionOption.OpenIfExists);
                CommonConfig.DatabaseFolder = await mainFolder.CreateFolderAsync(PortablePath.Combine("v2", "db"), CreationCollisionOption.OpenIfExists);
                CommonConfig.AttachmentsFolder = await mainFolder.CreateFolderAsync(PortablePath.Combine("Caches", "v2", "att"), CreationCollisionOption.OpenIfExists);
                CommonConfig.Logger = new ConsoleAndFileLogger();
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

                Dialogs.Initialize();

                ((ConsoleAndFileLogger)CommonConfig.Logger).CleanUpOldLogFiles();
                await DatabaseUtils.InitializeDatabases();

                PlatformConfig.SSLCertificateVerificationManager = new SSLCertificateVerificationManager();
                PlatformConfig.Preferences = preferences;
                PlatformConfig.ReachabilityReceiver = new ReachabilityReceiver();
                PlatformConfig.MessengerHub = new TinyMessengerHub();

                UNUserNotificationCenter.Current.Delegate = this;
            }).Wait();
        }

        bool InitializePlatform(UIApplication application)
        {
            return Task.Run(async () =>
            {
                var authenticator = AuthenticatorFactory.Create();
                if (!await authenticator.IsAuthenticatedAsync())
                {
                    CommonConfig.Logger.Info($"Writing required file system storage version...");
                    await FileSystemStorageUpdater.WriteRequiredStorageVersion();
                    CommonConfig.Logger.Info($"User was not authenticated - will present {nameof(LoginViewController)}");

                    return false;
                }

                CommonConfig.Logger.Info("Updating file system storage...");
                var updated = await FileSystemStorageUpdater.UpdateStorage();
                CommonConfig.Logger.Info(updated ? "File system storage updated" : "File system storage update not required");

                CommonConfig.Logger.Info($"User is authenticated - initializing...");
                var ci = await authenticator.GetConnectionInfoAsync();
                CommonConfig.Logger.Info($"Current connection info: {ci}");

                switch (ci.SslMode)
                {
                    case SslMode.AllowSelfSigned:
                        PlatformConfig.SSLCertificateVerificationManager.EnableSelfSignedCertificates();
                        break;
                    default:
                        PlatformConfig.SSLCertificateVerificationManager.DisableSelfSignedCertificates();
                        break;
                }

                CommonConfig.Logger.Info($"Initializing {nameof(Managers)}...");
                Managers.Initialize(ci);
                Managers.DocumentsManager.MaxToFetch = PlatformConfig.Preferences.DocumentsToDownload;
                Managers.DocumentsManager.DocumentBodyTypeRequest = PlatformConfig.Preferences.DocumentBodyRequestType;
                Managers.NotificationsManager.DocumentBodyTypeRequest = PlatformConfig.Preferences.DocumentBodyRequestType;
                Managers.SearchManager.DocumentBodyTypeRequest = PlatformConfig.Preferences.DocumentBodyRequestType;
                var policies = Managers.DownloadManager.DownloadPolicies;
                policies[ObjectType.Document] = new DownloadFoldersPolicy();
                if (PlatformConfig.Preferences.SynchroniseContacts)
                {
                    policies[ObjectType.Contact] = new DownloadAllPolicy();
                }
                if (PlatformConfig.Preferences.SynchroniseShortcodes)
                {
                    policies[ObjectType.Shortcode] = new DownloadAllPolicy();
                }

                if (PlatformConfig.Preferences.ClearCache)
                {
                    CommonConfig.Logger.Info("Clearing cache...");
                    await DatabaseUtils.ResetDatabases();
                    PlatformConfig.Preferences.ClearCache = false;
                    CommonConfig.Logger.Info("Cleared cache");
                }

                if (await Managers.CleanUpManager.IsCleanUpNecessary(PlatformConfig.Preferences.CleanCacheIntervalDays))
                {
                    CommonConfig.Logger.Info("Cleaning up cache....");
                    await Managers.CleanUpManager.CleanUp();
                    CommonConfig.Logger.Info("Cleaned up cache");
                }

                CommonConfig.Logger.Info($"Starting {nameof(IDownloadManager)} and {nameof(IOutgoingDocumentsManager)}...");
                await Managers.DownloadManager.Start();
                await Managers.OutgoingDocumentsManager.Start();

                CommonConfig.Logger.Info($"Refreshing reachability status...");
                await CommonConfig.ReachabilityService.Refresh();

                LocalNotificationsListener.Initialize();

                CommonConfig.Logger.Info($"Registering {nameof(ReachabilityReceiver)}...");
                PlatformConfig.ReachabilityReceiver.Register();

                CommonConfig.Logger.Info("Retrieving system settings...");
                ServerConfig.SystemSettings = await Managers.SystemManager.GetSystemSettingsAsync(SourceType.Local);

                BeginInvokeOnMainThread(() =>
                {
                    CommonConfig.Logger.Info("Refreshing APNS token...");

                    UIApplication.SharedApplication.ApplicationIconBadgeNumber = 0;

                    UNUserNotificationCenter.Current.RequestAuthorization(UNAuthorizationOptions.Alert | UNAuthorizationOptions.Badge | UNAuthorizationOptions.Sound, (result, error) =>
                    {
                        if (result)
                        {
                            BeginInvokeOnMainThread(() =>
                            {
                                application.RegisterForRemoteNotifications();
                            });
                        }
                        else
                        {
                            CommonConfig.Logger.Error(new NSErrorException(error));
                        }
                    });
                });

                CommonConfig.Logger.Info($"Initialized - will present {nameof(SplitMainViewController)}");

                return true;
            }).Result;
        }
    }
}

