using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Provider;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Com.Airbnb.Lottie;
using Java.IO;
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
using Mark5.Mobile.Droid.Utilities.DeviceReminder;
using Mark5.Mobile.Droid.Utilities.Workers;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Crashes;
using TinyIoC;

namespace Mark5.Mobile.Droid.Ui.Activities
{
    [Activity(MainLauncher = true, Icon = "@mipmap/ic_icon", Theme = "@style/mark5Splash", ScreenOrientation = ScreenOrientation.Portrait,
        NoHistory = true, ResizeableActivity = true, Name = "com.nordic_it.mark5.android.SplashActivity")]
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

            HandleSendIntent(this.Intent);

        }

        #region Sharing options handling
        private void HandleSendIntent(Intent intent)
        {
            string action = intent.Action;
            string type = intent.Type;

            if (Intent.ActionSend.Equals(action) && type != null)
            {
                ((Mark5Application)ApplicationContext).StartedFromShareOptions = true;

                if (type.Equals("text/plain"))
                {
                    HandleSendText(intent);
                }
                else if (type.StartsWith("image/") || type.Equals("text/x-vcard") || type.StartsWith("application/"))
                {
                    HandleSendFile(intent);
                }
            }
            else if (Intent.ActionSendMultiple.Equals(action) && type != null)
            {
                ((Mark5Application)ApplicationContext).StartedFromShareOptions = true;

                if (type.StartsWith("image/") || type.StartsWith("application/") || type.Equals("*/*"))
                {
                    HandleSendMultipleFiles(intent); 
                }
            }
        }

        private void HandleSendFile(Intent intent)
        {
            var contactUri = (Android.Net.Uri)intent.GetParcelableExtra(Intent.ExtraStream);

            var fullPathToSave = GetFilePath(contactUri);
          
            var fileStream = ContentResolver.OpenInputStream(contactUri);

            if(TrySaveSharedFile(fullPathToSave, fileStream) == true)
            {
                StartActivity(ComposeDocumentActivity.CreateIntent(this, DocumentCreationModeFlag.New, CopyToNewOption.None,
                    false, DocumentDirection.None, null, null, null, null, null, new List<Uri>() { new Uri(fullPathToSave) }));
            }

        }

        private void HandleSendText(Intent intent)
        {
            string sharedText = intent.GetStringExtra(Intent.ExtraText);
            if (sharedText != null)
            {
                StartActivity(ComposeDocumentActivity.CreateIntent(this, DocumentCreationModeFlag.New, CopyToNewOption.None,
                 false, DocumentDirection.None, null, null, null, sharedText));
            }
        }

        private void HandleSendMultipleFiles(Intent intent)
        {
            var fileUris = intent.GetParcelableArrayListExtra(Intent.ExtraStream);
            var uriList = new List<Uri>();
            if (fileUris != null)
            {
                foreach (var uri in fileUris)
                {
                    if (!(uri is Android.Net.Uri fileUri))
                        continue;

                    var fullPathToSave = GetFilePath(fileUri);

                    var fileStream = ContentResolver.OpenInputStream(fileUri);

                    if (TrySaveSharedFile(fullPathToSave, fileStream) == true)
                    {
                        uriList.Add(new Uri(fullPathToSave));
                    }
                }
            }

            if (uriList.Any())
            {
                StartActivity(ComposeDocumentActivity.CreateIntent(this, DocumentCreationModeFlag.New, CopyToNewOption.None,
                    false, DocumentDirection.None, null, null, null, null, null, uriList));
            }

        }

        private string GetFilePath(Android.Net.Uri uri)
        {
            string name = null;

            var cursor = ContentResolver.Query(uri, new string[] { OpenableColumns.DisplayName, OpenableColumns.Size }, null, null, null);
            if (cursor != null)
            {
                try
                {
                    if (cursor.MoveToFirst())
                    {
                        name = cursor.GetString(0);
                    }
                }
                finally
                {
                    cursor.Close();
                }
            }
            var newName = ReplaceIllegalCharacters(name);
            var folderName = Path.Combine(CommonConfig.AttachmentsFolder.Path);
            var folderPath = Path.Combine(folderName, Guid.NewGuid().ToString());

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var fullPathToSave = Path.Combine(folderPath, newName);

            return fullPathToSave;

        }

        private static bool TrySaveSharedFile(string fullPathToSave, Stream fileStream)
        {
            try
            {
                var uri2 = new Java.Net.URI("file://" + fullPathToSave);
                Java.IO.File file = new Java.IO.File(uri2);
                OutputStream output = new FileOutputStream(file);
                byte[] buffer = new byte[4 * 1024]; // or other buffer size
                int read;

                while ((read = fileStream.Read(buffer)) != 0)
                {
                    output.Write(buffer, 0, read);
                }

                output.Flush();

                return true;

            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
                return false;
            }
        }

        private static string ReplaceIllegalCharacters(string illegal)
        {
            var invalidFileNameChars = Path.GetInvalidFileNameChars();
            var invalidPathChars = Path.GetInvalidPathChars();
            var invalidChars = new char[] { '"', '»', '«', '\'', ' ' };
            string regexSearch = new string(invalidFileNameChars) + new string(invalidPathChars) + new string(invalidChars);
            Regex r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
            return r.Replace(illegal, "");
        }

        #endregion

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

                UpdateTokenInPreferences();
                
                CommonConfig.Logger.Info($"Initialized - will present {nameof(MainActivity)}");

                return true;
            }).ContinueWith(t =>
            {
                if (((Mark5Application)ApplicationContext).StartedFromShareOptions == true)
                    return;

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

        void UpdateTokenInPreferences()
        {
            try
            {
                TinyIoCContainer.Current.Resolve<IPushNotificationsRegistrator>().UpdateToken();
            }
            catch (Exception)
            {

            }
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