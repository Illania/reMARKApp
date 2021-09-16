using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using BackgroundTasks;
using Firebase.CloudMessaging;
using Firebase.Core;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Authenticator;
using Mark5.Mobile.Common.Database;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Job;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Model.HubMessages;
using Mark5.Mobile.Common.Service;
using Mark5.Mobile.Common.Storage;
using Mark5.Mobile.Common.Storage.AppFileStorage;
using Mark5.Mobile.Common.Storage.AppFileStorage.Enum;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.IOS.Common.ShareExtension;
using Mark5.Mobile.IOS.PushNotifications;
using Mark5.Mobile.IOS.Service;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.ViewControllers;
using Mark5.Mobile.IOS.Ui.ViewControllers.CalendarViews;
using Mark5.Mobile.IOS.Ui.ViewControllers.ComposeDocumentView;
using Mark5.Mobile.IOS.Utilities;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Crashes;
using Microsoft.Identity.Client;
using ModernHttpClient;
using TinyIoC;
using TinyMessenger;
using UIKit;
using UserNotifications;

namespace Mark5.Mobile.IOS
{
    [Register("AppDelegate")]
    public class AppDelegate : UIApplicationDelegate, IUNUserNotificationCenterDelegate, IMessagingDelegate
    {
        private IPushNotificationsRegistrator pushNotificationsRegistrator;
        const string backgroundTaskID = "com.nordic-it.mark5.mobile.ios.task";
        private const string syncFusionLicenseKey = "MzU3NTc2QDMxMzgyZTMzMmUzMGNVUXBkU3N4ZU1RbE5OS21KNjRaY2cxakVwVDhzejlObjJPOXV3ZWdHQUk9";
        DateTime lastForegroundTaskRunDate = DateTime.MinValue;

        public override UIWindow Window { get; set; }
        private bool isLoggedIn = false;


        public override bool WillFinishLaunching(UIApplication application, NSDictionary launchOptions)
        {
            try
            {
                var startupTime = new Stopwatch();
                startupTime.Start();

                InitializeCommon();

                App.Configure(); //Firebase Analytics
                Messaging.SharedInstance.Delegate = this;

                CommonConfig.Logger.Info("reMARK initializing...");
                isLoggedIn = InitializePlatform(application);
                CommonConfig.Logger.Info("reMARK initialized");

                if (isLoggedIn)
                {
                    if (ServerConfig.SystemSettings?.SystemInfo?.NewPushNotificationsSystemAvailable == true)
                    {
                        TinyIoCContainer.Current.Register<IPushNotificationsRegistrator>(new ANHRegistrator());
                        CommonConfig.Logger.Info("using ANH registrator...");
                    }
                    else
                    {
                        TinyIoCContainer.Current.Register<IPushNotificationsRegistrator>(new FCMRegistrator());
                        CommonConfig.Logger.Info("using FCM registrator...");
                    }
                    pushNotificationsRegistrator = TinyIoCContainer.Current.Resolve<IPushNotificationsRegistrator>();
                }
                    
                
#if DEBUG
                AnalyticsConfiguration.SharedInstance.SetAnalyticsCollectionEnabled(false);
#else
                AnalyticsConfiguration.SharedInstance.SetAnalyticsCollectionEnabled(PlatformConfig.Preferences.EnableReporting);
#endif

                Window = new UIWindow(UIScreen.MainScreen.Bounds);
                Window.ApplyTheme();

                UIViewController vc;

                if (!isLoggedIn)
                    vc = new LoginViewController();
                else if (Integration.IsIPad() && !Integration.IsiOSApplicationOnMac())
                    vc = new SplitMainViewController();
                else if (Integration.IsiOSApplicationOnMac())
                    vc = new TransitionViewController();
                else
                    vc = new SimpleMainViewController();

                Window.RootViewController = vc;
                startupTime.Stop();

                CommonConfig.Logger.Info($"Total startup time: {startupTime.ElapsedMilliseconds}ms");
            }
            catch (Exception ex)
            {
                CommonConfig.Logger?.Error(ex);
            }

            return false; // Always return false to pass handling of notifications to FinishedLaunching
        }

        //This function is necessary for Microsoft Authentication
        public override bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options)
        {
            if (url.Scheme == "remark.share.url" || url.Scheme == "remark.share.text")
            {

                UIViewController vc;
                try
                {
                    List<NSUrl> urlList = new();
                    SharedContentInsertType insertType = SharedContentInsertType.File;
                    if (url.Scheme == "remark.share.url")
                    {
                        var data = url.AbsoluteString.Replace("remark.share.url://", "");
                        var pathArray = data.Split(';');

                        foreach (var path in pathArray)
                        {
                            urlList.Add(new NSUrl(path));
                        }

                    }
                    else if(url.Scheme == "remark.share.text")
                    {
                        insertType = SharedContentInsertType.Text;
                        var textFilePath = url.AbsoluteString.Replace("remark.share.text://", "");
                        urlList.Add(new NSUrl(textFilePath));
                    }


                    if (Integration.IsIPad())
                        vc = new SplitMainViewController(new SharingOptions(insertType, urlList));
                    else
                        vc = new SimpleMainViewController(new SharingOptions(insertType, urlList));

                    Window.RootViewController = new NavigationController(vc, UIModalPresentationStyle.PageSheet);

                    return true;
                }
                catch(Exception ex)
                {
                    vc = new LoginViewController();
                    Window.RootViewController = vc;
                    return true;
                }
            } 
               
         
            
            AuthenticationContinuationHelper.SetAuthenticationContinuationEventArgs(url);
            return true;
        }

        public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {

            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(syncFusionLicenseKey);

            Crashes.GetErrorAttachments =
                    report => { return new[] { ErrorAttachmentLog.AttachmentWithText(SystemReportCollector.CreateLogReport(), "deviceLogs.txt") }; };
            AppCenter.Start("8aec5b28-2ac5-4956-997c-4867ef65d957", typeof(Crashes));

#if DEBUG
            Crashes.SetEnabledAsync(false);
#else
            Crashes.SetEnabledAsync(PlatformConfig.Preferences.EnableReporting);
#endif

#if ENABLE_TEST_CLOUD
            Xamarin.Calabash.Start();
#endif

            var OneDayInterval = 60 * 24;
            UIApplication.SharedApplication.SetMinimumBackgroundFetchInterval(OneDayInterval);

            Window.MakeKeyAndVisible();

            //Check note on background task down
            //if (UIDevice.CurrentDevice.CheckSystemVersion(13, 0))
            //    BGTaskScheduler.Shared.Register(backgroundTaskID, null, HandleBackgroundTask);

            if (launchOptions == null)
                return true;

            try
            {
                if (!UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
                {
                    var userInfo = (NSDictionary)launchOptions.ObjectForKey(UIApplication.LaunchOptionsRemoteNotificationKey);
                    if (userInfo != null)
                    {
                        var n = userInfo.ConvertToNotification();

                        if (n != null && n.ObjectType == ObjectType.Document)
                        {
                            var vc = new DocumentViewController();
                            vc.SetRefreshDataOnAppear();
                            vc.SetData(n.ObjectId);
                            Window.RootViewController.PresentViewController(new NavigationController(vc,
                                UIModalPresentationStyle.PageSheet), true, null);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error(ex);
            }

            return true;
        }

        private void TryRequestAuthorization()
        {
            try
            {
                pushNotificationsRegistrator.RequestAuthorization();
            }
            catch (NSErrorException nex)
            {
                CommonConfig.Logger.Error($"Error while requesting authorization", nex);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Error while requesting authorization", ex);
            }
        }

        public override bool ShouldSaveApplicationState(UIApplication application, NSCoder coder) => true;

        public override bool ShouldRestoreApplicationState(UIApplication application, NSCoder coder)
        {
            var stateCreationDate = (NSDate)coder.DecodeObject(UIApplication.StateRestorationTimestampKey);
            var yesterday = NSDate.Now.AddSeconds(-1 * 24 * 60 * 60);
            if (stateCreationDate.Compare(yesterday) == NSComparisonResult.Ascending)
                return false;

            var bundleVersion = coder.DecodeObject(UIApplication.StateRestorationBundleVersionKey);
            if (NSBundle.MainBundle.InfoDictionary["CFBundleVersion"] != bundleVersion)
                return false;

            var systemVersion = ((NSString)coder.DecodeObject(UIApplication.StateRestorationSystemVersionKey)).ToString();
            if (UIDevice.CurrentDevice.SystemVersion != systemVersion)
                return false;

            var userInterfaceIdiom = (UIUserInterfaceIdiom)((NSNumber)coder.DecodeObject(UIApplication.StateRestorationUserInterfaceIdiomKey)).Int32Value;
            if (UIDevice.CurrentDevice.UserInterfaceIdiom != userInterfaceIdiom)
                return false;

#if DEBUG
            return false;
#else
            return true;
#endif
        }

        public override UIViewController GetViewController(UIApplication application, string[] restorationIdentifierComponents, NSCoder coder)
        {
            var lastComponent = restorationIdentifierComponents.LastOrDefault();

            if (lastComponent == nameof(SimpleMainViewController))
                return Window.RootViewController;

            if (lastComponent == nameof(SplitMainViewController))
                return Window.RootViewController;

            if (lastComponent == "NavigationController_" + nameof(SearchCriteriaViewController))
                return new DarkNavigationController
                {
                    ModalPresentationStyle = UIModalPresentationStyle.FullScreen,
                    ModalTransitionStyle = UIModalTransitionStyle.CrossDissolve,
                    RestorationIdentifier = "NavigationController_" + nameof(SearchCriteriaViewController)
                };

            return null;
        }

        public override void OnActivated(UIApplication application)
        {
            LocalAuthenticationManager.NotifyApplicationActivated();

            Services.DocumentsUploadService?.Start();
            Services.DocumentPreviewsDownloadService?.Start();
            Services.DocumentsDownloadService?.Start();
            Services.ActionSyncService?.Start();

            HandleForegroundTask();
        }

        public override void DidEnterBackground(UIApplication application)
        {
            Services.DocumentsUploadService?.Stop();
            Services.DocumentPreviewsDownloadService?.Stop();
            Services.DocumentsDownloadService?.Stop();
            Services.ActionSyncService?.Start();

            LocalAuthenticationManager.NotifyApplicationEnteredBackground();

            //Check note on background task down
            //if (UIDevice.CurrentDevice.CheckSystemVersion(13, 0))
            //    ScheduleBackgroundTask();
        }

        public override void ReceiveMemoryWarning(UIApplication application)
        {
            CommonConfig.Logger.Warning("Received memory warning!");

            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);
        }

        #region Notification handling
        /// <summary>
        /// Indicates that a call to RegisterForRemoteNotifications() succeeded.
        /// </summary>
        /// <param name="application">Reference to the UIApplication that invoked this delegate method.</param>
        /// <param name="deviceToken">Received token.</param>
        public override async void RegisteredForRemoteNotifications(UIApplication application, NSData deviceToken)
        {
            CommonConfig.Logger.Info($"Received APNS token: {deviceToken}");

            var serviceVersion = ServerConfig.SystemSettings?.SystemInfo?.ServiceVersion;

            if (deviceToken == null)
            {
                CommonConfig.Logger.Error("deviceToken is null!");
                return;
            }

            await pushNotificationsRegistrator.RegisterToken(deviceToken);

            if (serviceVersion == null)
            {
                CommonConfig.Logger.Info($"It is not possible to update the APNS token because the server version is null");
                return;
            }

            bool notificationsInChinaEnabled = ServerConfig.SystemSettings?.SystemInfo?.NotificationsInChina == true;


            if (serviceVersion.CompareTo(new Version(3, 1, 5)) >= 0 && !notificationsInChinaEnabled && !(pushNotificationsRegistrator is ANHRegistrator))
            {
                CommonConfig.Logger.Info($"Not sending the APNS token because the current service version is equal or higher than 3.1.5 and Notifications Not Enabled in China");
                return;
            }

            //create and update APNS token (for PushSharp)
            string newToken = string.Empty;
            
            try
            {
                //formats the string as hexadecimal characters.
               newToken = string.Join("", deviceToken.Select(b => b.ToString("x2")));
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Error while parsing deviceToken", ex);
                return;
            }

            pushNotificationsRegistrator.UpdateToken(newToken);
        }

      

        public override void FailedToRegisterForRemoteNotifications(UIApplication application, NSError error)
        {
            CommonConfig.Logger.Error("Failed to received APNS Token", new NSErrorException(error));
            PlatformConfig.Preferences.PushNotificationToken = string.Empty;
        }

        [Export("messaging:didReceiveRegistrationToken:")]
        //Firebase Messaging delegate
        public void DidReceiveRegistrationToken(Messaging messaging, string pushToken)
        {
            //Should be called only for fcm registrator
            if (!(pushNotificationsRegistrator is FCMRegistrator))
                return;

            CommonConfig.Logger.Info($"Received FCM token: {pushToken}");

            var serviceVersion = ServerConfig.SystemSettings?.SystemInfo?.ServiceVersion;

            if (serviceVersion == null)
            {
                CommonConfig.Logger.Info($"It is not possible to update the push notification token because the server version is null");
                return;
            }

            if (serviceVersion.CompareTo(new Version(3, 1, 5)) < 0)
            {
                CommonConfig.Logger.Info($"Not sending the FCM token because the current service version is less than 3.1.5");
                return ;
            }

            if (ServerConfig.SystemSettings?.SystemInfo?.NotificationsInChina == true)
            {
                CommonConfig.Logger.Info($"Not sending the FCM token because the current service is using Chinese Notifications");
                return;
            }

            pushNotificationsRegistrator.UpdateToken(pushToken);
        }

        // iOS 10+, called when presenting notification
        //Asks the delegate how to handle a notification that arrived while the app was running in the foreground
       [Export("userNotificationCenter:willPresentNotification:withCompletionHandler:")]
        public void WillPresentNotification(UNUserNotificationCenter center, UNNotification notification, Action<UNNotificationPresentationOptions> options)
        {
            try
            {
                if (notification?.Request?.Identifier == LocalNotificationsListener.DocumentFailedToSendIdentifier)
                {
                    options(UNNotificationPresentationOptions.Alert);
                    return;
                }

                if (DeviceReminderNotificationManager.GetReminderInfo(notification) != null)
                {
                    options(UNNotificationPresentationOptions.Alert);
                    return;
                }

                var n = notification.ConvertToNotification();

                if (n.ObjectType == ObjectType.Document)
                {
                    if (notification.Request.Identifier != LocalNotificationsListener.DocumentFailedToSendIdentifier)
                        CommonConfig.MessengerHub.Publish(new NewNotificationsReceivedMessage(this));

                    options(UNNotificationPresentationOptions.Alert);
                }
                else
                {
                    options(UNNotificationPresentationOptions.None);
                }
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error(ex);

                options(UNNotificationPresentationOptions.None);
            }
        }

        // Called after the user selects an action from a notification from the app(iOS 10+).
        [Export("userNotificationCenter:didReceiveNotificationResponse:withCompletionHandler:")]
        public void DidReceiveNotificationResponse(UNUserNotificationCenter center, UNNotificationResponse response, Action completionHandler)
        {
             try
             {
                 if (response?.Notification?.Request?.Identifier == LocalNotificationsListener.DocumentFailedToSendIdentifier)
                 {
                     completionHandler();
                     return;
                 }


                 var reminderInfo = DeviceReminderNotificationManager.GetReminderInfo(response?.Notification);
                 if (reminderInfo != null)
                 {
                     var vc = new AppointmentViewController(reminderInfo.Value.CalendarId,
                         reminderInfo.Value.AppointmentId,
                         reminderInfo.Value.RecurrenceIndex, false);

                     Window.RootViewController.PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
                 }

                 var n = response.Notification.ConvertToNotification();

                 if (n.ObjectType == ObjectType.Document)
                 {
                     var vc = new DocumentViewController();
                     vc.SetRefreshDataOnAppear();
                     vc.SetData(n.ObjectId);

                     // we want to remove the previous document view controller in case user is opening emails from notifications - one after another.
                     if (Window.RootViewController.PresentedViewController != null && Window.RootViewController.PresentedViewController is NavigationController)
                     {
                         var navController = Window.RootViewController.PresentedViewController as NavigationController;
                         if (navController.TopViewController is DocumentViewController)
                         {
                             navController.DismissViewController(false, null);
                         }
                     }

                     Window.RootViewController.PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
                 }
             }
             catch (Exception ex)
             {
                 CommonConfig.Logger.Error(ex);
             }
             finally
             {
                 completionHandler();
             }
            
        }

        public void OnAuthorizationRequestCompleted(bool result, NSError error)
        {
            if(pushNotificationsRegistrator == null)
            {
                if (ServerConfig.SystemSettings?.SystemInfo?.NewPushNotificationsSystemAvailable == true)
                {
                    TinyIoCContainer.Current.Register<IPushNotificationsRegistrator>(new ANHRegistrator());
                }
                else
                {
                    TinyIoCContainer.Current.Register<IPushNotificationsRegistrator>(new FCMRegistrator());
                }
                pushNotificationsRegistrator = TinyIoCContainer.Current.Resolve<IPushNotificationsRegistrator>();
            }
            pushNotificationsRegistrator.OnAuthorizationRequestCompleted(result, error);
        }

        //This needs to be implemented to support silent notifications. 
        //Let's not forget the silent notifications limitations (2-3 notifications per hour max, and other limitations per day)
        //[Export("application:didReceiveRemoteNotification:fetchCompletionHandler:")]
        //public override void DidReceiveRemoteNotification(UIApplication application, NSDictionary userInfo, Action<UIBackgroundFetchResult> completionHandler)
        //
        //}

        #endregion


        #region Update Activities

        public override void PerformFetch(UIApplication application, Action<UIBackgroundFetchResult> completionHandler)
        {
            CommonConfig.Logger.Info("Background Fetch started...");

            Task.Run(async () =>
            {
                try
                {
                    await RunJobs();

                    completionHandler(UIBackgroundFetchResult.NewData);
                }
                catch (Exception ex)
                {
                    CommonConfig.Logger.Error("Background Fetch error!", ex);
                    completionHandler(UIBackgroundFetchResult.Failed);
                }
            });
        }

        //https://github.com/xamarin/xamarin-macios/issues/7456
        //Unfortunately we can't use BGTaskScheduler because there is a bug in Xamarin.
        //If we want to reactivate it we need also to add following to info.plist
        // 	<key>BGTaskSchedulerPermittedIdentifiers</key>
        //  <array>
        //      <string>com.nordic-it.mark5.mobile.ios.task</string>
        //  </array>
        void ScheduleBackgroundTask()
        {
            CommonConfig.Logger.Error("Scheduling background task ");

            var request = new BGAppRefreshTaskRequest(backgroundTaskID);
            request.EarliestBeginDate = NSDate.Now.AddSeconds(60 * 60); //1 hour

            BGTaskScheduler.Shared.Submit(request, out var _error);

            if (_error != null)
                CommonConfig.Logger.Error($"Error while scheduling background task: {_error} ");
        }

        void HandleBackgroundTask(BGTask task)
        {
            Task.Run(async () =>
            {
                try
                {
                    CommonConfig.Logger.Info("Running background task ");
                    await RunJobs();
                    task.SetTaskCompleted(true);
                }
                catch (Exception ex)
                {
                    CommonConfig.Logger.Error("Background task error!", ex);
                    task.SetTaskCompleted(false);
                }
            });

            //This is necessary because the task otherwise is not run periodically
            ScheduleBackgroundTask();
        }

        void HandleForegroundTask()
        {
            if (lastForegroundTaskRunDate > DateTime.Now.AddHours(-1))
                return;

            lastForegroundTaskRunDate = DateTime.Now;

            Task.Run(async () =>
            {
                try
                {
                    CommonConfig.Logger.Info("Running foreground task ");
                    await RunJobs();
                }
                catch (Exception ex)
                {
                    CommonConfig.Logger.Error("Foreground task error!", ex);
                }
            });
        }

        async Task RunJobs()
        {
            if (!await AuthenticatorFactory.Create().IsAuthenticatedAsync())
                return;

            var job1 = Jobs.SystemSettingsUpdateJob.Run();
            var job2 = Jobs.RemindersUpdateJob.Run();
            await Task.WhenAll(job1, job2);
        }


#endregion

        void InitializeCommon()
        {
            Task.Run(async () =>
            {
                var mainFolder = FileSystem.Current.LocalStorage;

                var preferences = new Preferences();

                if (preferences.ResetOnLaunch)
                    Integration.ClearData();

                CommonConfig.PathSeparator = Path.DirectorySeparatorChar;
                CommonConfig.DataFolder = await mainFolder.CreateFolderAsync(Path.Combine("v2", "data"), CreationCollisionOption.OpenIfExists);
                CommonConfig.DatabaseFolder = await mainFolder.CreateFolderAsync(Path.Combine("v2", "db"), CreationCollisionOption.OpenIfExists);
                CommonConfig.AttachmentsFolder = await mainFolder.CreateFolderAsync(Path.Combine("Caches", "v2", "att"), CreationCollisionOption.OpenIfExists);
                CommonConfig.DocumentsToUploadFolder = await mainFolder.CreateFolderAsync(Path.Combine("v2", "documents_upload"), CreationCollisionOption.OpenIfExists);
                CommonConfig.DocumentWorkingCopyFolder = await mainFolder.CreateFolderAsync(Path.Combine("v2", "document_work"), CreationCollisionOption.OpenIfExists);
                CommonConfig.RetainedDataFolder = await mainFolder.CreateFolderAsync("retained", CreationCollisionOption.OpenIfExists);
                CommonConfig.Logger = new ConsoleAndFileLogger();
                CommonConfig.DeviceInfoProvider = new DeviceInfoProvider();
                CommonConfig.HttpClientHandler = () => new NativeMessageHandler { AutomaticDecompression = Config.AcceptedResponseCompression };
                CommonConfig.OnStartTransmission = ActivityIndicator.Show;
                CommonConfig.OnStopTransmission = ActivityIndicator.Hide;
                CommonConfig.MessengerHub = new TinyMessengerHub();
                CommonConfig.Phonebook = new Phonebook();
                CommonConfig.Reachability = Reachability.Instance;
                CommonConfig.UsageAnalytics = new UsageAnalytics();
                CommonConfig.ConcurrentQueueType = typeof(PortableConcurrentQueue<>);
                CommonConfig.TimeZoneInfoDeserializer = TimeZoneInfo.FromSerializedString;
                CommonConfig.DeviceReminderNotificationManager = new DeviceReminderNotificationManager();

                if (UIDevice.CurrentDevice.CheckSystemVersion(10, 3))
                    CommonConfig.Utf8Normalizer = filename =>
                    {
                        var url = NSUrl.FromFilename(filename);
                        var fsPtr = url.GetFileSystemRepresentationAsUtf8Ptr;
                        var numBytes = 0;
                        while (Marshal.ReadByte(fsPtr, numBytes) != 0)
                            numBytes++;

                        var utf8Bytes = new byte[numBytes];
                        Marshal.Copy(fsPtr, utf8Bytes, 0, numBytes);
                        return Encoding.UTF8.GetString(utf8Bytes).SafeSubstringAfterLast(Path.DirectorySeparatorChar);
                    };
                else
                    CommonConfig.Utf8Normalizer = filename => filename;

#if !DEBUG
                CommonConfig.Logger.Level = Mark5.Mobile.Common.Utilities.LogLevel.INFO;
#else
                CommonConfig.Logger.Level = Mark5.Mobile.Common.Utilities.LogLevel.DEBUG;
#endif

                Dialogs.Initialize();

                ((ConsoleAndFileLogger)CommonConfig.Logger).CleanUpOldLogFiles();
                await DatabaseUtils.InitializeDatabases();

                PlatformConfig.SSLCertificateVerificationManager = new SSLCertificateVerificationManager();
                PlatformConfig.Preferences = preferences;
                PlatformConfig.ReachabilityReceiver = new ReachabilityReceiver();

                UNUserNotificationCenter.Current.Delegate = this;
            })
            .Wait();
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

                CommonConfig.UsageAnalytics.SetUserProperty(UserProperty.Hostname, ci.Hostname);
                CommonConfig.UsageAnalytics.SetUserProperty(UserProperty.SSL, ci.SslMode.ToString());
        
                PlatformConfig.SSLCertificateVerificationManager.DisableSelfSignedCertificates();                 

                CommonConfig.Logger.Info($"Initializing {nameof(Managers)}...");
                Managers.Initialize(ci, PlatformConfig.Preferences.AzureApplicationProxyBearerToken);
                Managers.DocumentsManager.MaxToFetch = PlatformConfig.Preferences.DocumentsToDownload;
                Managers.DocumentsManager.DocumentBodyTypeRequest = PlatformConfig.Preferences.DocumentBodyRequestType;
                Managers.NotificationsManager.DocumentBodyTypeRequest = PlatformConfig.Preferences.DocumentBodyRequestType;
                Managers.SearchManager.DocumentBodyTypeRequest = PlatformConfig.Preferences.DocumentBodyRequestType;

                if (PlatformConfig.Preferences.ClearCache)
                {
                    CommonConfig.UsageAnalytics.LogEvent(new SettingsCacheCleanUpEvent());

                    CommonConfig.Logger.Info("Clearing cache...");
                    await DatabaseUtils.ResetDatabases();
                    PlatformConfig.Preferences.ClearCache = false;
                    CommonConfig.Logger.Info("Cleared cache");
                }

                ServerConfig.SystemSettings = await Managers.SystemManager.GetSystemSettingsAsync(SourceType.Local);

                if (await Managers.CleanUpManager.IsCleanUpNecessary(PlatformConfig.Preferences.CleanCacheIntervalDays))
                {
                    CommonConfig.Logger.Info("Cleaning up cache....");
                    await Managers.CleanUpManager.CleanUp();
                    CommonConfig.Logger.Info("Cleaned up cache");
                }

                CommonConfig.Logger.Info($"Refreshing reachability status...");
                _ = CommonConfig.Reachability.Refresh();

                LocalNotificationsListener.Initialize();

                CommonConfig.Logger.Info($"Registering {nameof(ReachabilityReceiver)}...");
                PlatformConfig.ReachabilityReceiver.Register();

                CommonConfig.Logger.Info("Retrieving system settings...");

                if (!String.IsNullOrEmpty(ServerConfig.SystemSettings.SystemInfo.CustomerName))
                    CommonConfig.UsageAnalytics.SetUserProperty(UserProperty.CustomerName, ServerConfig.SystemSettings.SystemInfo.CustomerName);

                DateTimeConverter.UseServerTimezone = PlatformConfig.Preferences.UseServerTimezone;

                BeginInvokeOnMainThread(() =>
                {
                    CommonConfig.Logger.Info("Refreshing APNS token...");

                    TryRequestAuthorization();
                });

                CommonConfig.Logger.Info($"Initialized - will present {nameof(AbstractMainViewController)}");

                return true;
            }).Result;
        }
 
    }
}