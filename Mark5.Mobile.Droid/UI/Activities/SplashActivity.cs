using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using HockeyApp.Android;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Authenticator;
using Mark5.Mobile.Common.Database;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Storage;
using Mark5.Mobile.Droid.Services;
using Mark5.Mobile.Droid.Utilities;
using Mark5.Mobile.Droid.Utilities.Hockey;

namespace Mark5.Mobile.Droid.Ui.Activities
{
    [Activity(Label = "MARK5", MainLauncher = true, Icon = "@mipmap/ic_icon", Theme = "@style/mark5Splash", ScreenOrientation = ScreenOrientation.Portrait, NoHistory = true)]
    public class SplashActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.activity_splash);

            var uiOptions = (int) Window.DecorView.SystemUiVisibility;
            uiOptions |= (int) SystemUiFlags.Immersive;
            uiOptions |= (int) SystemUiFlags.HideNavigation;
            Window.DecorView.SystemUiVisibility = (StatusBarVisibility) uiOptions;

            if (CommonConfig.Logger.IsInfoEnabled())
                CommonConfig.Logger.Info($"Created {nameof(SplashActivity)}");
        }

        protected override void OnStart()
        {
            base.OnStart();

            CommonConfig.Logger.Info($"Starting {nameof(SplashActivity)}...");

#if !DEBUG
            CrashManager.Register(this, PlatformConfig.HockeyId, new CustomCrashManagerListener());
            CrashManager.ResetAlwaysSend(new Java.Lang.Ref.WeakReference(this));
#endif

            Task.Run(async () =>
                {
                    var authenticator = AuthenticatorFactory.Create();
                    if (!await authenticator.IsAuthenticatedAsync())
                    {
                        CommonConfig.Logger.Info($"Writing required file system storage version...");

                        await FileSystemStorageUpdater.WriteRequiredStorageVersion();

                        CommonConfig.Logger.Info($"User was not authenticated - will present {nameof(LoginActivity)}");

                        return false;
                    }

                    CommonConfig.Logger.Info("Updating file system storage...");

                    var updated = await FileSystemStorageUpdater.UpdateStorage();

                    CommonConfig.Logger.Info(updated ? "File system storage updated" : "File system storage update not required");

                    CommonConfig.Logger.Info($"User is authenticated - initializing...");

                    var ci = await authenticator.GetConnectionInfoAsync();

                    CommonConfig.Logger.Info($"Current connection info: {ci}");
                    CommonConfig.Logger.Info($"Push token: {PlatformConfig.Preferences.PushNotificationToken}");

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

                    CommonConfig.Logger.Info($"Starting {nameof(IDocumentsDownloadManager)} and {nameof(IOutgoingDocumentsManager)}...");

                    await Managers.DocumentsDownloadManager.Start();
                    await Managers.OutgoingDocumentsManager.Start();

                    CommonConfig.Logger.Info($"Refreshing reachability status...");
                    await CommonConfig.ReachabilityService.Refresh();

                    CommonConfig.Logger.Info($"Registering {nameof(ReachabilityBroadcastReceiver)}...");
                    PlatformConfig.ReachabilityBroadcastReceiver.Register();

                    CommonConfig.Logger.Info("Retrieving system settings...");

                    ServerConfig.SystemSettings = await Managers.SystemManager.GetSystemSettingsAsync(SourceType.Local);

                    LocalNotificationsListener.Initialize();

                    DateTimeUtils.UseServerTimezone = PlatformConfig.Preferences.UseServerTimeZone;

                    CommonConfig.Logger.Info($"Initialized - will present {nameof(MainActivity)}");

                    return true;
                })
                .ContinueWith(t =>
                    {
                        if (t.Result)
                            StartActivity(new Intent(this, typeof(MainActivity)));
                        else
                            StartActivity(new Intent(this, typeof(LoginActivity)));
                    },
                    TaskScheduler.FromCurrentSynchronizationContext());

            CommonConfig.Logger.Info($"Started {nameof(SplashActivity)}");
        }
    }
}