//
// Project: Mark5.Mobile.Droid
// File: PreferenceFragment.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;
using Android.Media;
using Android.OS;
using Android.Provider;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Support.V7.App;
using Android.Support.V7.Preferences;
using Android.Text;
using Android.Text.Style;
using Firebase.Iid;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Authenticator;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Fragments
{

    public class PreferenceFragment : PreferenceFragmentCompat, PreferenceFragmentCompat.IOnPreferenceStartScreenCallback, ISharedPreferencesOnSharedPreferenceChangeListener
    {

        static class RequestCodes
        {
            public const int NotificationRingtoneRequest = 1;
        }

        public override Fragment CallbackFragment { get { return this; } }

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

        public override void OnActivityResult(int requestCode, int resultCode, Intent data)
        {
            if (resultCode == (int)Android.App.Result.Ok && requestCode == RequestCodes.NotificationRingtoneRequest)
            {
                var uri = data.GetParcelableExtra(RingtoneManager.ExtraRingtonePickedUri);
                PlatformConfig.Preferences.NotificationsRingtone = uri?.ToString() ?? string.Empty;
            }
        }

        public override void OnCreatePreferences(Bundle savedInstanceState, string rootKey)
        {
            SetPreferencesFromResource(Resource.Xml.preferences, rootKey);

            var versionPreference = FindPreference(GetString(Resource.String.pref_key_about_version));
            if (versionPreference != null)
            {
                versionPreference.Summary = CommonConfig.DeviceInfoProvider.GetAppVersionString();
            }

            Task.Run(() =>
            {
                return AuthenticatorFactory.Create().GetConnectionInfoAsync();
            }).ContinueWith(t =>
            {
                var ci = t.Result;

                var usernamePreference = FindPreference(GetString(Resource.String.pref_key_account_username));
                if (usernamePreference != null)
                {
                    usernamePreference.Summary = ci.Username;
                }

                var hostnamePreference = FindPreference(GetString(Resource.String.pref_key_account_hostname));
                if (hostnamePreference != null)
                {
                    hostnamePreference.Summary = ci.Hostname;
                }

                var portPreference = FindPreference(GetString(Resource.String.pref_key_account_port));
                if (portPreference != null)
                {
                    portPreference.Summary = ci.Port.ToString();
                }

                var sslPreference = FindPreference(GetString(Resource.String.pref_key_account_ssl));
                if (sslPreference != null)
                {
                    switch (ci.SslMode)
                    {
                        case SslMode.On:
                            sslPreference.Summary = GetString(Resource.String.ssl_on);
                            break;
                        case SslMode.AllowSelfSigned:
                            sslPreference.Summary = GetString(Resource.String.ssl_self_signed);
                            break;
                        default:
                            var summary = new SpannableString(GetString(Resource.String.ssl_off));
                            summary.SetSpan(new StyleSpan(TypefaceStyle.Bold), 0, summary.Length(), SpanTypes.ExclusiveInclusive);
                            summary.SetSpan(new ForegroundColorSpan(new Color(ContextCompat.GetColor(Context, Resource.Color.brown))), 0, summary.Length(), SpanTypes.ExclusiveInclusive);
                            sslPreference.SummaryFormatted = summary;
                            break;
                    }
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public override bool OnPreferenceTreeClick(Preference preference)
        {
            if (preference.Key == GetString(Resource.String.pref_key_contacts_synchronised) && !PlatformConfig.Preferences.SynchroniseContacts)
            {
                Dialogs.ShowYesNoDialog(Context, Resource.String.clear_contacts_cache_title, Resource.String.clear_contacts_cache_summary, async () =>
                {

                    var dismissAction = Dialogs.ShowInfiniteProgressDialog(Context, Resource.String.clearing_contacts_cache, Resource.String.please_wait);

                    try
                    {
                        await Managers.CleanUpManager.ClearContactsCache();
                        await Managers.CleanUpManager.CleanUp(new[] { ModuleType.Contacts });

                        dismissAction();
                    }
                    catch (Exception ex)
                    {
                        dismissAction();

                        CommonConfig.Logger.Error("Could not clear contacts cache!", ex);

                        await Dialogs.ShowErrorDialogAsync(Activity, ex);
                    }
                });
            }

            if (preference.Key == GetString(Resource.String.pref_key_shortcodes_synchronised) && !PlatformConfig.Preferences.SynchroniseShortcodes)
            {
                Dialogs.ShowYesNoDialog(Context, Resource.String.clear_shortcodes_cache_title, Resource.String.clear_shortcodes_cache_summary, async () =>
                {
                    var dismissAction = Dialogs.ShowInfiniteProgressDialog(Context, Resource.String.clearing_shortcodes_cache, Resource.String.please_wait);

                    try
                    {
                        await Managers.CleanUpManager.ClearShortcodeCache();
                        await Managers.CleanUpManager.CleanUp(new[] { ModuleType.Shortcodes });

                        dismissAction();
                    }
                    catch (Exception ex)
                    {
                        dismissAction();

                        CommonConfig.Logger.Error("Could not clear shortcodes cache!", ex);

                        await Dialogs.ShowErrorDialogAsync(Activity, ex);
                    }
                });
            }

            if (preference.Key == GetString(Resource.String.pref_key_notification_ringtone))
            {
                var i = new Intent(RingtoneManager.ActionRingtonePicker);
                i.PutExtra(RingtoneManager.ExtraRingtoneType, (int)RingtoneType.Notification);
                i.PutExtra(RingtoneManager.ExtraRingtoneTitle, GetString(Resource.String.pref_notification_ringtone_title));
                i.PutExtra(RingtoneManager.ExtraRingtoneDefaultUri, Settings.System.DefaultNotificationUri);
                if (!string.IsNullOrWhiteSpace(PlatformConfig.Preferences.NotificationsRingtone))
                {
                    i.PutExtra(RingtoneManager.ExtraRingtoneExistingUri, Android.Net.Uri.Parse(PlatformConfig.Preferences.NotificationsRingtone));
                }
                StartActivityForResult(i, RequestCodes.NotificationRingtoneRequest);
                return true;
            }

            if (preference.Key == GetString(Resource.String.pref_key_about_send_feedback))
            {
                var sendIntent = new Intent();
                sendIntent.SetAction(Intent.ActionSendto);
                sendIntent.SetData(Android.Net.Uri.Parse("mailto:support@nordic-it.com?subject=MARK5%20for%20Android%20Feedback"));
                StartActivity(sendIntent);

                return true;
            }

            if (preference.Key == GetString(Resource.String.pref_key_advanced_create_system_report))
            {
                var dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, Resource.String.dialog_creating_report, Resource.String.please_wait);

                Task.Run(() =>
                {
                    return SystemReportCollector.CreateFullReport();
                }).ContinueWith(t =>
                {
                    dismissAction();

                    if (!t.IsFaulted)
                    {
                        StartActivity(SystemReportCollector.CreateShareReportIntent(Activity, t.Result));
                    }
                }, TaskScheduler.FromCurrentSynchronizationContext());

                return true;
            }

            if (preference.Key == GetString(Resource.String.pref_key_advanced_update_config))
            {
                var dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, Resource.String.dialog_update_config_title, Resource.String.please_wait);
                Task.Run(async () =>
                            {
                                try
                                {
                                    FirebaseInstanceId.Instance?.DeleteInstanceId();
                                    var _nullToken = FirebaseInstanceId.Instance?.Token; // Token will be null, but it will cause refresh
                                }
                                catch (Exception ex)
                                {
                                    CommonConfig.Logger.Error("Could not reset Firebase token!", ex);
                                }

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

                                    await Dialogs.ShowErrorDialogAsync(Activity, ex);
                                }
                            });

                return true;
            }

            if (preference.Key == GetString(Resource.String.pref_key_account_logout))
            {
                Dialogs.ShowYesNoDialog(Activity, Resource.String.dialog_logout_title, Resource.String.dialog_logout_content, async () =>
                {
                    Dialogs.ShowInfiniteProgressDialog(Activity, Resource.String.dialog_logging_out_title, Resource.String.please_wait);

                    try
                    {
                        if (!string.IsNullOrWhiteSpace(PlatformConfig.Preferences.PushNotificationToken))
                            await Managers.NotificationsManager.UnSubscribe(DeviceType.Android, PlatformConfig.Preferences.PushNotificationToken);
                    }
                    catch
                    {
                    }

                    Integration.ClearDataAndStop();
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
            {
                Managers.DocumentsManager.MaxToFetch = PlatformConfig.Preferences.DocumentsToDownload;
            }
            if (key == GetString(Resource.String.pref_key_documents_download_as_plaintext))
            {
                Managers.DocumentsManager.DocumentBodyTypeRequest = PlatformConfig.Preferences.DocumentBodyRequestType;
                Managers.NotificationsManager.DocumentBodyTypeRequest = PlatformConfig.Preferences.DocumentBodyRequestType;
                Managers.SearchManager.DocumentBodyTypeRequest = PlatformConfig.Preferences.DocumentBodyRequestType;
            }
            if (key == GetString(Resource.String.pref_key_search_documents_to_get))
            {
                Managers.SearchManager.MaxDocumentsToFetch = PlatformConfig.Preferences.MaxDocumentsToSearch;
            }

            if (key == GetString(Resource.String.pref_key_search_contacts_to_get))
            {
                Managers.SearchManager.MaxContactsToFetch = PlatformConfig.Preferences.MaxContactsToSearch;
            }
            if (key == GetString(Resource.String.pref_key_search_shortcodes_to_get))
            {
                Managers.SearchManager.MaxShortcodesToFetch = PlatformConfig.Preferences.MaxShortcodesToSearch;
            }
            if (key == GetString(Resource.String.pref_key_contacts_synchronised))
            {
                if (PlatformConfig.Preferences.SynchroniseContacts)
                {
                    Managers.DownloadManager.DownloadPolicies[ObjectType.Contact] = new DownloadAllPolicy();
                }
                else
                {
                    Managers.DownloadManager.DownloadPolicies.Remove(ObjectType.Contact);
                }
            }
            if (key == GetString(Resource.String.pref_key_shortcodes_synchronised))
            {
                if (PlatformConfig.Preferences.SynchroniseShortcodes)
                {
                    Managers.DownloadManager.DownloadPolicies[ObjectType.Shortcode] = new DownloadAllPolicy();
                }
                else
                {
                    Managers.DownloadManager.DownloadPolicies.Remove(ObjectType.Shortcode);
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
    }
}
