using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Runtime;
using Android.Support.V7.App;
using Firebase.Analytics;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Authenticator;
using Mark5.Mobile.Common.Database;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Storage.AppFileStorage;
using Mark5.Mobile.Common.Storage.AppFileStorage.Enum;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Service;
using Mark5.Mobile.Droid.Utilities;
using Mark5.Mobile.Droid.Utilities.DeviceReminder;
using TinyMessenger;
using Xamarin.Android.Net;

namespace Mark5.Mobile.Droid
{
    [Application(Theme = "@style/mark5")]
    public class Mark5Application : Application
    {
        public bool StartedFromRoot { get; set; }

        public bool StartedFromShareOptions { get; set; }

        public ApplicationLifecycleHandler LifecycleHandler;

        public Mark5Application(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
            ThreadPool.SetMinThreads(50, 50);
            ThreadPool.SetMaxThreads(100, 100);
        }

        public override void OnCreate()
        {
            base.OnCreate();

            AppCompatDelegate.CompatVectorFromResourcesEnabled = true;

            LifecycleHandler = new ApplicationLifecycleHandler();
            RegisterActivityLifecycleCallbacks(LifecycleHandler);

            Task.Run(async () =>
                {
                    var mainFolder = FileSystem.Current.LocalStorage;

                    CommonConfig.PathSeparator = Path.DirectorySeparatorChar;
                    CommonConfig.DataFolder = await mainFolder.CreateFolderAsync(Path.Combine("data", "data"), CreationCollisionOption.OpenIfExists);
                    CommonConfig.DatabaseFolder = await mainFolder.CreateFolderAsync(Path.Combine("data", "db"), CreationCollisionOption.OpenIfExists);
                    CommonConfig.AttachmentsFolder = await mainFolder.CreateFolderAsync(Path.Combine("..", "cache", "att"), CreationCollisionOption.OpenIfExists);
                    CommonConfig.DocumentsToUploadFolder = await mainFolder.CreateFolderAsync(Path.Combine("data", "documents_upload"), CreationCollisionOption.OpenIfExists);
                    CommonConfig.DocumentWorkingCopyFolder = await mainFolder.CreateFolderAsync(Path.Combine("data", "document_work"), CreationCollisionOption.OpenIfExists);
                    CommonConfig.RetainedDataFolder = await mainFolder.CreateFolderAsync("retained", CreationCollisionOption.OpenIfExists);
                    CommonConfig.Logger = new SimpleLogger();
                    CommonConfig.DeviceInfoProvider = new DeviceInfoProvider();
                    CommonConfig.HttpClientHandler = () => new AndroidClientHandler { AutomaticDecompression = Config.AcceptedResponseCompression };
                    CommonConfig.MessengerHub = new TinyMessengerHub();
                    CommonConfig.Phonebook = new Phonebook();
                    CommonConfig.Reachability = Reachability.Instance;
                    CommonConfig.ConcurrentQueueType = typeof(PortableConcurrentQueue<>);
                    CommonConfig.Utf8Normalizer = s => s;
                    CommonConfig.UsageAnalytics = new UsageAnalytics(FirebaseAnalytics.GetInstance(this));
                    CommonConfig.TimeZoneInfoDeserializer = TimeZoneInfo.FromSerializedString;
                    CommonConfig.DeviceReminderNotificationManager = new DeviceReminderNotificationManager();

#if !DEBUG
                    CommonConfig.Logger.Level = LogLevel.INFO;
#else
                    CommonConfig.Logger.Level = LogLevel.DEBUG;
#endif

                    await DatabaseUtils.InitializeDatabases();

                    PlatformConfig.SSLCertificateVerificationManager = new SSLCertificateVerificationManager();
                    PlatformConfig.ReachabilityMonitor = new ReachabilityMonitor();
                    PlatformConfig.CallStateBroadcastReceiver = new CallStateBroadcastReceiver();
                    PlatformConfig.DeviceReminderBroadcastReceiver = new DeviceReminderBroadcastReceiver();
                    PlatformConfig.Preferences = new Preferences();

                    var authenticator = AuthenticatorFactory.Create();
                    if (await authenticator.IsAuthenticatedAsync())
                    {
                        var ci = await authenticator.GetConnectionInfoAsync();

                        PlatformConfig.SSLCertificateVerificationManager.DisableSelfSignedCertificates();
                        
                        Managers.Initialize(ci);
                        Managers.DocumentsManager.MaxToFetch = PlatformConfig.Preferences.DocumentsToDownload;
                        Managers.DocumentsManager.DocumentBodyTypeRequest = PlatformConfig.Preferences.DocumentBodyRequestType;
                        Managers.NotificationsManager.DocumentBodyTypeRequest = PlatformConfig.Preferences.DocumentBodyRequestType;
                        Managers.SearchManager.DocumentBodyTypeRequest = PlatformConfig.Preferences.DocumentBodyRequestType;

                        ServerConfig.SystemSettings = await Managers.SystemManager.GetSystemSettingsAsync(SourceType.Local);
                    }
                })
                .Wait();

            CommonConfig.Logger.Info($"Initialized {nameof(Mark5Application)}");
        }

        public override void OnTerminate()
        {
            base.OnTerminate();

            CommonConfig.Logger.Info($"Terminated {nameof(Mark5Application)}");
        }
    }
}