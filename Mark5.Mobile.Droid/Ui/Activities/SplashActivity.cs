using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Com.Airbnb.Lottie;
using Firebase.Iid;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Authenticator;
using Mark5.Mobile.Common.Database;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Service;
using Mark5.Mobile.Common.Storage;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Service;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Utilities;
#if !DEBUG
using HockeyApp.Android;
using Mark5.Mobile.Droid.Utilities.Hockey;
#endif

namespace Mark5.Mobile.Droid.Ui.Activities
{
    [Activity(MainLauncher = true, Icon = "@mipmap/ic_icon", Theme = "@style/mark5Splash", ScreenOrientation = ScreenOrientation.Portrait, NoHistory = true, ResizeableActivity = true)]
    public class SplashActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.activity_splash);

            var uiOptions = (int)Window.DecorView.SystemUiVisibility;
            uiOptions |= (int)SystemUiFlags.Immersive;
            uiOptions |= (int)SystemUiFlags.HideNavigation;
            Window.DecorView.SystemUiVisibility = (StatusBarVisibility)uiOptions;

            if (CommonConfig.Logger.IsInfoEnabled())
                CommonConfig.Logger.Info($"Created {nameof(SplashActivity)}");

            ((Mark5Application)ApplicationContext).StartedFromRoot = true;

        }

        protected override void OnStart()
        {
            base.OnStart();

            var openedFromNotification = Intent?.Extras?.ContainsKey("title") == true;
            if (openedFromNotification && !IsTaskRoot)
            {
                ProcessNotification();
                return;
            }

            CommonConfig.Logger.Info($"Starting {nameof(SplashActivity)}...");

#if !DEBUG
            CrashManager.Register(this, Config.HockeyId, new CustomCrashManagerListener());
            CrashManager.ResetAlwaysSend(new Java.Lang.Ref.WeakReference(this));

            Firebase.Analytics.FirebaseAnalytics.GetInstance(this).SetAnalyticsCollectionEnabled(PlatformConfig.Preferences.EnableReporting);
#else
            Firebase.Analytics.FirebaseAnalytics.GetInstance(this).SetAnalyticsCollectionEnabled(false);
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

                RunOnUiThread(() =>
                {
                    var animationView = FindViewById<LottieAnimationView>(Resource.Id.animation_view);

                    animationView.Progress = 1;
                    animationView.Animate().Alpha(1f).SetDuration(200);
                });

                CommonConfig.Logger.Info("Updating file system storage...");

                var updated = await FileSystemStorageUpdater.UpdateStorage();

                CommonConfig.Logger.Info(updated ? "File system storage updated" : "File system storage update not required");

                CommonConfig.Logger.Info($"User is authenticated - initializing...");

                var ci = await authenticator.GetConnectionInfoAsync();

                CommonConfig.UsageAnalytics.SetUserProperty(UserProperty.Hostname, ci.Hostname);
                CommonConfig.UsageAnalytics.SetUserProperty(UserProperty.SSL, ci.SslMode.ToString());

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
                    CommonConfig.UsageAnalytics.LogEvent(new SettingsCacheCleanUpEvent());

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

                CommonConfig.Logger.Info($"Refreshing reachability status...");
                await CommonConfig.Reachability.Refresh();

                CommonConfig.Logger.Info($"Registering {nameof(ReachabilityMonitor)}...");
                PlatformConfig.ReachabilityMonitor.Register(ApplicationContext);

                if (PlatformConfig.Preferences.CallerIdentificationEnabled)
                {
                    CommonConfig.Logger.Info($"Registering {nameof(CallStateBroadcastReceiver)}...");
                    PlatformConfig.CallStateBroadcastReceiver.Register();
                }

                CommonConfig.Logger.Info("Retrieving system settings...");
                ServerConfig.SystemSettings = await Managers.SystemManager.GetSystemSettingsAsync(SourceType.Local);

                if (!String.IsNullOrEmpty(ServerConfig.SystemSettings.SystemInfo.CustomerName))
                    CommonConfig.UsageAnalytics.SetUserProperty(UserProperty.CustomerName, ServerConfig.SystemSettings.SystemInfo.CustomerName);

                SystemSettingsJobService.ScheduleJob();

                LocalNotificationsListener.Initialize();

                DateTimeConverter.UseServerTimezone = PlatformConfig.Preferences.UseServerTimeZone;

                if (!string.IsNullOrWhiteSpace(FirebaseInstanceId.Instance.Token))
                    PlatformConfig.Preferences.PushNotificationToken = FirebaseInstanceId.Instance.Token;

                CommonConfig.Logger.Info($"Initialized - will present {nameof(MainActivity)}");

                return true;
            }).ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    Exception ex = t.Exception;
                    CommonConfig.Logger.Error("Splash OnStart() Exception : ", ex);
                    Dialogs.SendCriticalReport(this, ex);
                    return;
                }

                Services.DocumentsUploadService?.Start();
                Services.DocumentPreviewsDownloadService?.Start();
                Services.DocumentsDownloadService?.Start();

                PushNotificationsUtilities.CreateChannelIfNotExists(this);

                if (t.Result)
                    StartActivity(MainActivity.CreateIntent(this));
                else
                    ShowLoginButton();

                if (openedFromNotification && IsTaskRoot)
                    ProcessNotification();

            }, TaskScheduler.FromCurrentSynchronizationContext());

            CommonConfig.Logger.Info($"Started {nameof(SplashActivity)}");
        }

        void ProcessNotification()
        {
            var not = PushNotificationsConverter.ExtractNotification(Intent.Extras);
            PushNotificationsUtilities.ProcessBackgroundNotificationClicked(this, not);
        }

        void ShowLoginButton()
        {
            var animationView = FindViewById<LottieAnimationView>(Resource.Id.animation_view);
            var progressBar = FindViewById<ProgressBar>(Resource.Id.progress_bar);
            var loginButton = FindViewById<AppCompatButton>(Resource.Id.login_button);

            loginButton.Click += (sender, e) => StartActivity(LoginActivity.CreateIntent(this));

            progressBar.Visibility = ViewStates.Gone;
            loginButton.Visibility = ViewStates.Visible;

            animationView.Alpha = 1f;
            animationView.PlayAnimation();
        }
    }
}