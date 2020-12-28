using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Com.Airbnb.Lottie;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Authenticator;
using Mark5.Mobile.Common.Database;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Service;
using Mark5.Mobile.Common.Storage;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Service;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Utilities;
using Mark5.Mobile.Droid.Utilities.DeviceReminder;
using Mark5.Mobile.Droid.Utilities.Workers;
using ME.Pushy.Sdk;
using Microsoft.AppCenter.Crashes;

namespace Mark5.Mobile.Droid.Ui.Activities
{
    [Activity(MainLauncher = true, Icon = "@mipmap/ic_icon", Theme = "@style/mark5Splash", ScreenOrientation = ScreenOrientation.Portrait, NoHistory = true, ResizeableActivity = true)]
    public class SplashActivity : AppCompatActivity
    {
        const string CalendarIdKey = "calendarId";
        const string AppointmentIdKey = "appointmentId";
        const string RecurrenceIndexKey = "recurrenceIndex";

        public static Intent CreateShowAppointmentIntent(Context context, int calendarId, int appointmentId, int recurrenceIndex)
        {
            var intent = new Intent(context, typeof(SplashActivity));

            intent.PutExtra(CalendarIdKey, calendarId);
            intent.PutExtra(AppointmentIdKey, appointmentId);
            intent.PutExtra(RecurrenceIndexKey, recurrenceIndex);

            return intent;
        }

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
            Crashes.GetErrorAttachments =
                report => { return new[] { ErrorAttachmentLog.AttachmentWithText(SystemReportCollector.CreateLogCatReport(), "deviceLogs.txt") }; };
            AppCenter.Start(Config.AppCenterId, typeof(Crashes));

            Firebase.Analytics.FirebaseAnalytics.GetInstance(this).SetAnalyticsCollectionEnabled(PlatformConfig.Preferences.EnableReporting);
#else
            Firebase.Analytics.FirebaseAnalytics.GetInstance(this).SetAnalyticsCollectionEnabled(false);
#endif

            Task.Run(async () =>
            {
                await Crashes.SetEnabledAsync(PlatformConfig.Preferences.EnableReporting);

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

                    if (animationView != null)
                    {
                        animationView.Progress = 1;
                        animationView.Animate().Alpha(1f).SetDuration(200);
                    }

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
                _ = CommonConfig.Reachability.Refresh();

                CommonConfig.Logger.Info($"Registering {nameof(ReachabilityMonitor)}...");
                PlatformConfig.ReachabilityMonitor.Register(ApplicationContext);

                if (PlatformConfig.Preferences.CallerIdentificationEnabled)
                {
                    CommonConfig.Logger.Info($"Registering {nameof(CallStateBroadcastReceiver)}...");
                    PlatformConfig.CallStateBroadcastReceiver.Register();
                }

                if (!string.IsNullOrEmpty(ServerConfig.SystemSettings.SystemInfo.CustomerName))
                    CommonConfig.UsageAnalytics.SetUserProperty(UserProperty.CustomerName, ServerConfig.SystemSettings.SystemInfo.CustomerName);

                SystemSettingsWorker.Schedule();

                LocalNotificationsListener.Initialize();

                DateTimeConverter.UseServerTimezone = PlatformConfig.Preferences.UseServerTimeZone;

                if (!Pushy.IsRegistered(this))
                    await PushNotificationsUtilities.RegisterForPushNotifications(this);

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
                Services.ActionSyncService?.Start();

                DeviceReminderWorker.Schedule();

                PushNotificationsUtilities.CreateChannelIfNotExists(this);
                DeviceReminderBroadcastReceiver.CreateChannelIfNotExists(this);

                if (t.Result)
                {
                    Intent intent = null;
                    if (Intent.HasExtra(CalendarIdKey))
                    {
                        var calendarId = Intent.GetIntExtra(CalendarIdKey, 0);
                        var appointmentId = Intent.GetIntExtra(AppointmentIdKey, 0);
                        var recurrenceIndex = Intent.GetIntExtra(RecurrenceIndexKey, 0);

                        intent = MainActivity.CreateShowAppointmentIntent(this, calendarId, appointmentId, recurrenceIndex);
                    }
                    else
                        intent = MainActivity.CreateIntent(this);

                    StartActivity(intent);
                }
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
            var loginButton = FindViewById<AppCompatButton>(Resource.Id.splash_login_button);

            loginButton.Click += (sender, e) => StartActivity(LoginActivity.CreateIntent(this));

            progressBar.Visibility = ViewStates.Gone;
            loginButton.Visibility = ViewStates.Visible;

            animationView.Alpha = 1f;
            animationView.PlayAnimation();
        }
    }
}