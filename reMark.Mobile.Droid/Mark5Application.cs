using Android.App;
using Android.Runtime;
using AndroidX.AppCompat.App;
using Firebase.Analytics;
using reMark.Mobile.Classes.Enum;
using reMark.Mobile.Common;
using reMark.Mobile.Common.Authenticator;
using reMark.Mobile.Common.Database;
using reMark.Mobile.Common.Manager;
using reMark.Mobile.Common.Storage.AppFileStorage.Enum;
using reMark.Mobile.Common.Utilities;
using reMark.Mobile.Droid.Service;
using reMark.Mobile.Droid.Utilities;
using TinyMessenger;
using Xamarin.Android.Net;
using Application = Android.App.Application;
using FileSystem = reMark.Mobile.Common.Storage.AppFileStorage.FileSystem;
using Preferences = reMark.Mobile.Droid.Utilities.Preferences;

namespace reMark.Mobile.Droid
{
    [Application(Theme = "@style/reMark")]
    public class reMarkApplication : Application
    {
        public bool StartedFromRoot { get; set; }

        public bool StartedFromShareOptions { get; set; }

        public ApplicationLifecycleHandler LifecycleHandler;

        public reMarkApplication(IntPtr javaReference, JniHandleOwnership transfer)
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
                    CommonConfig.EmlFolder = await mainFolder.CreateFolderAsync(Path.Combine("Eml", "v2", "att"), CreationCollisionOption.OpenIfExists);
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

#if !DEBUG
                    CommonConfig.Logger.Level = LogLevel.INFO;
#else
                    CommonConfig.Logger.Level = LogLevel.DEBUG;
#endif

                    await DatabaseUtils.InitializeDatabases();

                    PlatformConfig.SSLCertificateVerificationManager = new SSLCertificateVerificationManager();
                    PlatformConfig.ReachabilityMonitor = new ReachabilityMonitor();
                    PlatformConfig.CallStateBroadcastReceiver = new CallStateBroadcastReceiver();
                    PlatformConfig.Preferences = new Preferences();

                    var authenticator = AuthenticatorFactory.Create();
                    if (await authenticator.IsAuthenticatedAsync())
                    {
                        var ci = await authenticator.GetConnectionInfoAsync();

                        PlatformConfig.SSLCertificateVerificationManager.DisableSelfSignedCertificates();

                        Managers.Initialize(ci, PlatformConfig.Preferences.AzureApplicationProxyBearerToken,
                            new Classes.Azure.AzureApplicationProxyInfo()
                            {
                                AppClientId = PlatformConfig.Preferences.AzureApplicationProxyAppClientId,
                                ApplicationProxyClientId = PlatformConfig.Preferences.AzureApplicationProxyAppProxyId,
                                IsEnabled = PlatformConfig.Preferences.AzureApplicationProxyEnabled
                            });
                        Managers.DocumentsManager.MaxToFetch = PlatformConfig.Preferences.DocumentsToDownload;
                        Managers.DocumentsManager.DocumentBodyTypeRequest = PlatformConfig.Preferences.DocumentBodyRequestType;
                        Managers.NotificationsManager.DocumentBodyTypeRequest = PlatformConfig.Preferences.DocumentBodyRequestType;
                        Managers.SearchManager.DocumentBodyTypeRequest = PlatformConfig.Preferences.DocumentBodyRequestType;
                        ServerConfig.SystemSettings = await Managers.SystemManager.GetSystemSettingsAsync(SourceType.Local);
 
                    }
                })
                .Wait();

            CommonConfig.Logger.Info($"Initialized {nameof(reMarkApplication)}");
        }

        public override void OnTerminate()
        {
            base.OnTerminate();

            CommonConfig.Logger.Info($"Terminated {nameof(reMarkApplication)}");
        }
    }
}