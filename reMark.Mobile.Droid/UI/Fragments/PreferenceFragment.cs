using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;
using Android.Media;
using Android.OS;
using Android.Provider;
using Android.Text;
using Android.Text.Style;
using AndroidX.AppCompat.App;
using AndroidX.Core.Content;
using AndroidX.Core.Hardware.Fingerprint;
using AndroidX.Fragment.App;
using AndroidX.Preference;
using reMark.Mobile.Classes.Enum;
using reMark.Mobile.Common;
using reMark.Mobile.Common.Authenticator;
using reMark.Mobile.Common.Manager;
using reMark.Mobile.Common.Model;
using reMark.Mobile.Common.Utilities;
using reMark.Mobile.Droid.Service;
using reMark.Mobile.Droid.Ui.Activities;
using reMark.Mobile.Droid.Ui.Common;
using reMark.Mobile.Droid.Utilities;
using TinyIoC;
using Color = Android.Graphics.Color;
using DeviceType = reMark.Mobile.Common.Model.DeviceType;

namespace reMark.Mobile.Droid.Ui.Fragments
{
    public class PreferenceFragment : PreferenceFragmentCompat, PreferenceFragmentCompat.IOnPreferenceStartScreenCallback, ISharedPreferencesOnSharedPreferenceChangeListener
    {
        public override Fragment CallbackFragment => this;
        Action dismissAction;

        public static (PreferenceFragment fragment, string tag) NewInstance()
        {
            CommonConfig.UsageAnalytics.LogEvent(new OpenSettingsEvent());

            var fragment = new PreferenceFragment();
            var tag = $"{nameof(PreferenceFragment)}";

            return (fragment, tag);
        }

        public override void OnResume()
        {
            base.OnResume();

            var title = GetString(Resource.String.settings);
            var subtitle = PreferenceScreen.Title == title ? string.Empty : PreferenceScreen.Title;

            ((AppCompatActivity)Activity).SupportActionBar.Title = title;
            ((AppCompatActivity)Activity).SupportActionBar.Subtitle = subtitle;

            PreferenceManager.SharedPreferences.RegisterOnSharedPreferenceChangeListener(this);
        }

        public override void OnPause()
        {
            base.OnPause();

            PreferenceManager.SharedPreferences.UnregisterOnSharedPreferenceChangeListener(this);
        }

        public override void OnDestroyView()
        {
            dismissAction?.Invoke();
            base.OnDestroyView();
        }

        public override void OnActivityResult(int requestCode, int resultCode, Intent data)
        {
            if (resultCode == (int)Android.App.Result.Ok)
            {
                if (requestCode == RequestCodes.NotificationRingtoneRequest)
                {
                    var uri = data.GetParcelableExtra(RingtoneManager.ExtraRingtonePickedUri);
                    PlatformConfig.Preferences.NotificationsRingtone = uri?.ToString() ?? string.Empty;
                }
            }
            //The only way the user can return from the settings screen is by pressing back, hence requestCode = canceled.
            else if (resultCode == (int)Android.App.Result.Canceled)
            {
                if (Build.VERSION.SdkInt >= BuildVersionCodes.M && requestCode == RequestCodes.DrawOnTopRequest) //Only relevant if version is M or above.
                {
                    PlatformConfig.Preferences.CallerIdentificationEnabled = Settings.CanDrawOverlays(Context); //Sets the preference based on what the user selected in the settings screen for drawing overlays.
                    if (PlatformConfig.Preferences.CallerIdentificationEnabled)
                    {
                        PlatformConfig.CallStateBroadcastReceiver.Register();
                    }

                    Activity.OnBackPressed();
                }
            }
        }

        public override async void OnCreatePreferences(Bundle savedInstanceState, string rootKey)
        {
            SetPreferencesFromResource(Resource.Xml.preferences, rootKey);

            var versionPreference = FindPreference(GetString(Resource.String.pref_key_about_version));
            if (versionPreference != null)
                versionPreference.Summary = CommonConfig.DeviceInfoProvider.GetAppVersionString();

            var t = AuthenticatorFactory.Create().GetConnectionInfoAsync();

            var ci = await t;

            var usernamePreference = FindPreference(GetString(Resource.String.pref_key_account_username));
            if (usernamePreference != null)
                usernamePreference.Summary = ci.Username;

            var hostnamePreference = FindPreference(GetString(Resource.String.pref_key_account_hostname));
            if (hostnamePreference != null)
                hostnamePreference.Summary = ci.Hostname;

            var portPreference = FindPreference(GetString(Resource.String.pref_key_account_port));
            if (portPreference != null)
                portPreference.Summary = ci.Port.ToString();

            var sslPreference = FindPreference(GetString(Resource.String.pref_key_account_ssl));
            if (sslPreference != null)
                switch (ci.SslMode)
                {
                    case SslMode.On:
                        sslPreference.Summary = GetString(Resource.String.ssl_on);
                        break;
                    default:
                        var summary = new SpannableString(GetString(Resource.String.ssl_off));
                        summary.SetSpan(new StyleSpan(TypefaceStyle.Bold), 0, summary.Length(), SpanTypes.ExclusiveInclusive);
                        summary.SetSpan(new ForegroundColorSpan(new Color(ContextCompat.GetColor(Context, Resource.Color.brown))), 0, summary.Length(), SpanTypes.ExclusiveInclusive);
                        sslPreference.SummaryFormatted = summary;
                        break;
                }

            var swipeOptions = FindPreference(GetString(Resource.String.pref_key_swipe_options));
            if (swipeOptions != null)
            {
                swipeOptions.PreferenceClick += (object sender, Preference.PreferenceClickEventArgs e) =>
                {
                    SwipeActionsFragment swipeActionsFragment;
                    string swipeActionsFragmentTag;
                    var fragmentTransaction = Activity.SupportFragmentManager.BeginTransaction();
                    (swipeActionsFragment, swipeActionsFragmentTag) = SwipeActionsFragment.NewInstance();
                    fragmentTransaction.Replace(Resource.Id.fragment_container, swipeActionsFragment, swipeActionsFragmentTag);
                    fragmentTransaction.AddToBackStack(null);
                    fragmentTransaction.Commit();
                };
            }

            var sendingDelay = FindPreference(GetString(Resource.String.pref_key_sending_delay));
            var rememberLastUsedDelaySettings = FindPreference(GetString(Resource.String.pref_key_remember_last_user_delay_settings));
            if (!ServerConfig.SystemSettings.SystemInfo.DelaySendAvailable)
            {
                if(sendingDelay!=null && rememberLastUsedDelaySettings!=null)
                {
                    PreferenceScreen.RemovePreference(sendingDelay);
                    PreferenceScreen.RemovePreference(rememberLastUsedDelaySettings);
                }
            }
                
            var autoReplySettings = FindPreference(GetString(Resource.String.pref_key_autoreply));
            if (!ServerConfig.SystemSettings.SystemInfo.AutoReplyAvailable && autoReplySettings != null)
                PreferenceScreen.RemovePreference(autoReplySettings);
            else if(autoReplySettings != null)
            {
                autoReplySettings.PreferenceClick += async (object sender, Preference.PreferenceClickEventArgs e) =>
                {
                    var autoReplyRule = await Managers.DocumentsManager.GetAutoReplyRule();
                    AutoReplyFragment autoReplyFragment;
                    string autoReplyFragmentTag;
                    var fragmentTransaction = Activity.SupportFragmentManager.BeginTransaction();
                    (autoReplyFragment, autoReplyFragmentTag) = AutoReplyFragment.NewInstance(autoReplyRule);
                    fragmentTransaction.Replace(Resource.Id.fragment_container, autoReplyFragment, autoReplyFragmentTag);
                    fragmentTransaction.AddToBackStack(null);
                    fragmentTransaction.Commit();
                };
            }

            var syncUserActivities = FindPreference(GetString(Resource.String.pref_key_sync_user_activities));
            if (!ServerConfig.SystemSettings.SystemInfo.UserActivitiesAvailable && syncUserActivities!=null)
                PreferenceScreen.RemovePreference(syncUserActivities); 
      
            var extraFieldsOptions = FindPreference(GetString(Resource.String.pref_key_extra_fields_options));
            if (!ServerConfig.SystemSettings.SystemInfo.ExtraFieldsEditingAvailable && extraFieldsOptions != null)
                PreferenceScreen.RemovePreference(extraFieldsOptions);
            if (extraFieldsOptions != null)
            {
                extraFieldsOptions.PreferenceClick += ExtraFieldsOptions_PreferenceClick;
            }

            var setPresetCategory = FindPreference(GetString(Resource.String.pref_key_set_preset_category));
            if (setPresetCategory != null)
            {
                setPresetCategory.PreferenceClick += (object sender, Preference.PreferenceClickEventArgs e) =>
                {
                    PresetCategoryFragment presetFragment;
                    string presetFragmentTag;
                    var fragmentTransaction = Activity.SupportFragmentManager.BeginTransaction();
                    (presetFragment, presetFragmentTag) = PresetCategoryFragment.NewInstance();
                    fragmentTransaction.Replace(Resource.Id.fragment_container, presetFragment,presetFragmentTag);
                    fragmentTransaction.AddToBackStack(null);
                    fragmentTransaction.Commit();
                };
            }


            var syncFavorites = FindPreference(GetString(Resource.String.pref_key_sync_favorite_folders));
            var syncFavoritesOld = FindPreference(GetString(Resource.String.pref_key_sync_favorite_folders_old));
            if (!ServerConfig.SystemSettings.SystemInfo.SyncFavoritesWithDesktopAvailable && syncFavorites != null)
            {
                PreferenceScreen.RemovePreference(syncFavorites);
                if (syncFavoritesOld != null)
                    syncFavoritesOld.PreferenceChange += async (object sender, Preference.PreferenceChangeEventArgs e) => await HandleSync(e);

            }

            if (ServerConfig.SystemSettings.SystemInfo.SyncFavoritesWithDesktopAvailable && syncFavoritesOld != null)
            {
                PreferenceScreen.RemovePreference(syncFavoritesOld);
                if (syncFavorites != null)
                    syncFavorites.PreferenceChange += async (object sender, Preference.PreferenceChangeEventArgs e) => await HandleSync(e);
            }

            var serviceVersion = ServerConfig.SystemSettings?.SystemInfo?.ServiceVersion;
            if (serviceVersion == null || serviceVersion.CompareTo(new Version(3, 2, 0)) < 0)
            {
                var screen = (PreferenceScreen)FindPreference(GetString(Resource.String.pref_sync_favorites));
                if (screen != null)
                    PreferenceScreen.RemovePreference(screen);
            }
        }

        private async void ExtraFieldsOptions_PreferenceClick(object sender, Preference.PreferenceClickEventArgs e)
        {
            if (ServerConfig.SystemSettings.SystemInfo.ExtraFieldsEditingAvailable)
            {
                StartActivity(ExtraFieldsListActivity.CreateIntent(Context));
                return;
            }
            else
            {
                await Dialogs.ShowConfirmDialogAsync(Context, Resource.String.extra_fields_alert_not_available_title,
                                                        Resource.String.extra_fields_alert_not_available_content);
                return;
            }
        }

        async Task HandleSync(Preference.PreferenceChangeEventArgs e)
        {
            var newValue = int.Parse(e.NewValue.ToString());
            var selected = (FavoriteFoldersSyncType)newValue;
            if (selected == FavoriteFoldersSyncType.None)
                return;

            if (!ServerConfig.SystemSettings.SystemInfo.SyncFavoritesWithDesktopAvailable
                && selected == FavoriteFoldersSyncType.SyncWithDesktop)
            {         
                await Dialogs.ShowConfirmDialogAsync(this.Context, Resource.String.attention,Resource.String.sync_fav_folders_with_desktop_not_available);
                return;
            }

            try
            {
                Managers.FavoriteFoldersManager = selected ==
                      FavoriteFoldersSyncType.SyncWithDesktop
                      ? Managers.FavoriteFoldersDesktopSyncManager
                      : Managers.FavoriteFoldersDeviceSyncManager;

                var moduleFavoriteFoldersCollection = await Managers.FavoriteFoldersManager.GetServiceFavoriteFoldersAsync(retain: false);
                if (moduleFavoriteFoldersCollection.ModuleFavoriteFolders == null || PlatformConfig.Preferences.SyncFavoriteFolders ==
                    FavoriteFoldersSyncType.SyncWithDesktop)
                    await Managers.FavoriteFoldersManager.UpdateServiceFavoriteFoldersAsync();
                else
                {
                    ProcessFavoriteFoldersDeviceSyncOption(moduleFavoriteFoldersCollection);
                }
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Error while synchronizing favorite folders", ex);
                await Dialogs.ShowErrorDialogAsync(Activity, ex);
            }
        }


        async void ProcessFavoriteFoldersDeviceSyncOption(ModuleFavoriteFoldersCollection moduleFavoriteFoldersCollection)
        {
            var selectedOption = await Dialogs.ShowListDialog(Activity, Resource.String.sync_fav_folders_action_title,
                 $"{GetString(Resource.String.sync_fav_folders_action_description)} {moduleFavoriteFoldersCollection.UpdatedAt.ToShortDateString()}",
                 new string[] { GetString(Resource.String.sync_fav_folders_use_server), GetString(Resource.String.sync_fav_folders_use_device) }, true);

            if (selectedOption == 0)
            {
                foreach (var favorite in moduleFavoriteFoldersCollection.ModuleFavoriteFolders)
                    await Managers.FoldersManager.SetFavoriteFoldersAsync(favorite.ModuleType, favorite.Folders);

                var availableModules = new List<ModuleType> { ModuleType.Shortcodes, ModuleType.Contacts, ModuleType.Documents };
                await Managers.FoldersManager.ClearFavoritesAsync(availableModules.Except(moduleFavoriteFoldersCollection.ModuleFavoriteFolders.Select(mff => mff.ModuleType)).ToList());
            }
            else if (selectedOption == 1)
                await Managers.FavoriteFoldersManager.UpdateServiceFavoriteFoldersAsync();
            else
            {
                return;
            }
        }

        public override bool OnPreferenceTreeClick(Preference preference)
        {
            if (preference.Key == GetString(Resource.String.pref_key_sync_favorite_folders))
                return false;

            if (preference.Key == GetString(Resource.String.pref_key_documents_use_server_timezone))
                Dialogs.ShowConfirmDialog(Context, Resource.String.dialog_restart_required_title, Resource.String.dialog_restart_required_content);

            if (preference.Key == GetString(Resource.String.pref_key_notification_ringtone))
            {
                var i = new Intent(RingtoneManager.ActionRingtonePicker);
                i.PutExtra(RingtoneManager.ExtraRingtoneType, (int)RingtoneType.Notification);
                i.PutExtra(RingtoneManager.ExtraRingtoneTitle, GetString(Resource.String.pref_notification_ringtone_title));
                i.PutExtra(RingtoneManager.ExtraRingtoneDefaultUri, Settings.System.DefaultNotificationUri);
                if (!string.IsNullOrWhiteSpace(PlatformConfig.Preferences.NotificationsRingtone))
                    i.PutExtra(RingtoneManager.ExtraRingtoneExistingUri, Android.Net.Uri.Parse(PlatformConfig.Preferences.NotificationsRingtone));
                StartActivityForResult(i, RequestCodes.NotificationRingtoneRequest);
                return true;
            }

            if (preference.Key == GetString(Resource.String.pref_key_about_send_feedback))
            {
                dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, Resource.String.dialog_creating_report, Resource.String.please_wait);
                Task.Run(() => { return SystemReportCollector.CreateFullReport(); })
                    .ContinueWith(async t =>
                {
                    dismissAction();

                    if (!t.IsFaulted)
                    {
                        var sendWithReMARK = await Dialogs.ShowYesNoDialogAsync(Context, Resource.String.send_with_reMark_title, Resource.String.send_report_with_reMark_content);

                        if (sendWithReMARK)
                            StartActivity(SystemReportCollector.CreateShareFeedbackComposeDocumentActivityIntent(Context, t.Result));
                        else
                            StartActivity(SystemReportCollector.CreateShareFeedbackIntent(t.Result));

                    }

                }, TaskScheduler.FromCurrentSynchronizationContext());
                return true;
            }

            if (preference.Key == GetString(Resource.String.pref_key_auth))
            {
                var keyguardManager = (Android.App.KeyguardManager)Activity.GetSystemService(Context.KeyguardService);
                var fingerprintManager = FingerprintManagerCompat.From(Context);

                if (!keyguardManager.IsKeyguardSecure && !fingerprintManager.HasEnrolledFingerprints)
                    Dialogs.ShowConfirmDialog(Context, Resource.String.auth_not_enrolled_title, Resource.String.auth_not_enrolled_content);
            }

            if (preference.Key == GetString(Resource.String.pref_key_advanced_connection_diagnostics))
            {
                ConnectionDiagnosticsFragment connectionDiagnosticsFragment;
                string connectionDiagnosticsFragmentTag;
                (connectionDiagnosticsFragment, connectionDiagnosticsFragmentTag) = ConnectionDiagnosticsFragment.NewInstance();

                var fragmentTransaction = Activity.SupportFragmentManager.BeginTransaction();
                fragmentTransaction.Replace(Resource.Id.fragment_container, connectionDiagnosticsFragment, connectionDiagnosticsFragmentTag);
                fragmentTransaction.AddToBackStack(null);
                fragmentTransaction.Commit();
            }
            if (preference.Key == GetString(Resource.String.pref_key_advanced_update_config))
            {
                CommonConfig.UsageAnalytics.LogEvent(new SettingsUpdateSystemConfigurationEvent());

                dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, Resource.String.dialog_update_config_title, Resource.String.please_wait);
                Task.Run(async () =>
                {

                    try
                    {
                        var ss = await Managers.SystemManager.GetSystemSettingsAsync(SourceType.Remote);
                        ServerConfig.SystemSettings = ss;

                        await Managers.SystemManager.GetSystemUsersDepartmentsAsync(SourceType.Remote);

                        dismissAction();
                    }
                    catch (Exception ex)
                    {
                        dismissAction();

                        CommonConfig.Logger.Error("Could not retrieve system settings!", ex);

                        Activity.RunOnUiThread(async () =>
                        {
                            await Dialogs.ShowErrorDialogAsync(Activity, ex);
                        });
                    }
                });

                return true;
            }

            if (preference.Key == GetString(Resource.String.pref_key_account_logout))
            {
                Dialogs.ShowYesNoDialog(Activity,
                    Resource.String.dialog_logout_title,
                    Resource.String.dialog_logout_content,
                    async () =>
                    {
                        dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, Resource.String.dialog_logging_out_title, Resource.String.please_wait);

                        try
                        {
                            if (!string.IsNullOrWhiteSpace(PlatformConfig.Preferences.PushNotificationToken))
                                await Managers.NotificationsManager.UnSubscribe(DeviceType.Android, PlatformConfig.Preferences.PushNotificationToken);
                        }
                        catch (Exception ex)
                        {
                            CommonConfig.Logger.Error("Error while unsubscribing during log out!", ex);
                        }

                        await AuthenticatorFactory.Create().RetainConnectionInfoAsync();
                        await Integration.ClearData(Context);
                        dismissAction();

                        Dialogs.ShowBlockingAlert(Activity, Resource.String.please_restart);

                    });
                return true;
            }

            if (preference.Key == GetString(Resource.String.pref_key_about_privacy_policy))
            {
                var i = new Intent(Intent.ActionView, Android.Net.Uri.Parse("https://www.iubenda.com/privacy-policy/8086960"));
                StartActivity(i);
            }

            if (preference.Key == GetString(Resource.String.pref_key_about_version))
            {
                var i = new Intent(Settings.ActionApplicationDetailsSettings, Android.Net.Uri.Parse("package:" + Activity.PackageName));
                StartActivity(i);
            }

            return base.OnPreferenceTreeClick(preference);
        }

        public void OnSharedPreferenceChanged(ISharedPreferences sharedPreferences, string key)
        {
            if (key == GetString(Resource.String.pref_key_documents_to_load))
                Managers.DocumentsManager.MaxToFetch = PlatformConfig.Preferences.DocumentsToDownload;
            if (key == GetString(Resource.String.pref_key_documents_download_as_plaintext))
            {
                Managers.DocumentsManager.DocumentBodyTypeRequest = PlatformConfig.Preferences.DocumentBodyRequestType;
                Managers.NotificationsManager.DocumentBodyTypeRequest = PlatformConfig.Preferences.DocumentBodyRequestType;
                Managers.SearchManager.DocumentBodyTypeRequest = PlatformConfig.Preferences.DocumentBodyRequestType;
            }
            if (key == GetString(Resource.String.pref_key_callidentification_identification_enabled))
            {
                var valueChangedTo = PlatformConfig.Preferences.CallerIdentificationEnabled;
                if (valueChangedTo)
                {
                    if (Build.VERSION.SdkInt >= BuildVersionCodes.M) //If version is api 22 or above
                    {
                        Dialogs.ShowConfirmDialog(Context, Resource.String.redirect_to_draw_settings_title, Resource.String.redirect_to_draw_settings_content,
                                                () =>
                        {

                            var intent = new Intent(Settings.ActionManageOverlayPermission, Android.Net.Uri.Parse("package:" + Context.PackageName));
                            StartActivityForResult(intent, RequestCodes.DrawOnTopRequest);
                        });
                    }
                    else
                    {
                        Dialogs.ShowConfirmDialog(Context, Resource.String.must_save_persons_companies_offline_title, Resource.String.must_save_persons_companies_offline_content, null);
                        PlatformConfig.CallStateBroadcastReceiver.Register();
                    }
                }
                else
                {
                    PlatformConfig.CallStateBroadcastReceiver.Unregister();
                }
            }
        }

        public bool OnPreferenceStartScreen(PreferenceFragmentCompat caller, PreferenceScreen pref)
        {
            var args = new Bundle();
            args.PutString(ArgPreferenceRoot, pref.Key);
            var ft = ((AppCompatActivity)Activity).SupportFragmentManager.BeginTransaction();
            ft.SetCustomAnimations(Resource.Animation.enter_from_right, Resource.Animation.exit_to_left, Resource.Animation.enter_from_left, Resource.Animation.exit_to_right);
            ft.Replace(Resource.Id.fragment_container, new PreferenceFragment
            {
                Arguments = args
            });

            ft.AddToBackStack(pref.Key);
            ft.Commit();
            return true;
        }

        static class RequestCodes
        {
            public const int NotificationRingtoneRequest = 1;
            public const int DrawOnTopRequest = 2;
        }
    }
}