using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Runtime;
using Android.Support.V7.App;
using Firebase.Analytics;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Database;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Service;
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

            AppCompatDelegate.CompatVectorFromResourcesEnabled = true;

            Task.Run(async () =>
                {
                    var mainFolder = FileSystem.Current.LocalStorage;

                    CommonConfig.PathSeparator = Path.DirectorySeparatorChar;
                    CommonConfig.DataFolder = await mainFolder.CreateFolderAsync(PortablePath.Combine("data", "data"), CreationCollisionOption.OpenIfExists);
                    CommonConfig.DatabaseFolder = await mainFolder.CreateFolderAsync(PortablePath.Combine("data", "db"), CreationCollisionOption.OpenIfExists);
                    CommonConfig.AttachmentsFolder = await mainFolder.CreateFolderAsync(PortablePath.Combine("..", "cache", "att"), CreationCollisionOption.OpenIfExists);
                    CommonConfig.DocumentsToUploadFolder = await mainFolder.CreateFolderAsync(PortablePath.Combine("data", "documents_upload"), CreationCollisionOption.OpenIfExists);
                    CommonConfig.DocumentWorkingCopyFolder = await mainFolder.CreateFolderAsync(PortablePath.Combine("data", "document_work"), CreationCollisionOption.OpenIfExists);
                    CommonConfig.Logger = new SimpleLogger();
                    CommonConfig.DeviceInfoProvider = new DeviceInfoProvider();
                    CommonConfig.HttpClientHandler = () => new AndroidClientHandler { AutomaticDecompression = Config.AcceptedResponseCompression };
                    CommonConfig.MessengerHub = new TinyMessengerHub();
                    CommonConfig.Phonebook = new Phonebook();
                    CommonConfig.Reachability = new Reachability();
                    CommonConfig.ConcurrentQueueType = typeof(PortableConcurrentQueue<>);
                    CommonConfig.Utf8Normalizer = s => s;
                    CommonConfig.UsageAnalytics = new UsageAnalytics(FirebaseAnalytics.GetInstance(this));

#if !DEBUG
                    CommonConfig.Logger.Level = LogLevel.INFO;
#else
                    CommonConfig.Logger.Level = LogLevel.DEBUG;
#endif

                    await DatabaseUtils.InitializeDatabases();

                    PlatformConfig.SSLCertificateVerificationManager = new SSLCertificateVerificationManager();
                    PlatformConfig.ReachabilityBroadcastReceiver = new ReachabilityBroadcastReceiver();
                    PlatformConfig.Preferences = new Preferences();
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