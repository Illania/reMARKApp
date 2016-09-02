//
// Project: Mark5.Mobile.Droid
// File: PreferenceFragment.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Android.Content;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V7.App;
using Android.Support.V7.Preferences;
using Mark5.Mobile.Common;
using Mark5.Mobile.Droid.Utilities;
using Mark5.Mobile.Droid.Views.Common;
using Xamarin;
using Android.Provider;

namespace Mark5.Mobile.Droid.Views.Fragments
{

    public class PreferenceFragment : PreferenceFragmentCompat, PreferenceFragmentCompat.IOnPreferenceStartScreenCallback, ISharedPreferencesOnSharedPreferenceChangeListener
    {

        public override Fragment CallbackFragment
        {
            get
            {
                return this;
            }
        }

        public override void OnResume()
        {
            base.OnResume();

            ((AppCompatActivity)Activity).SupportActionBar.Title = PreferenceScreen.Title;

            PreferenceManager.SharedPreferences.RegisterOnSharedPreferenceChangeListener(this);
        }

        public override void OnPause()
        {
            base.OnPause();

            PreferenceManager.SharedPreferences.UnregisterOnSharedPreferenceChangeListener(this);
        }

        public override void OnCreatePreferences(Bundle savedInstanceState, string rootKey)
        {
            SetPreferencesFromResource(Resource.Xml.preferences, rootKey);

            var versionPreference = FindPreference(GetString(Resource.String.pref_key_about_version));
            if (versionPreference != null)
            {
                versionPreference.Summary = CommonConfig.DeviceInfoProvider.GetAppVersionString();
            }
        }

        public override bool OnPreferenceTreeClick(Preference preference)
        {
            if (preference.Key == GetString(Resource.String.pref_key_advanced_logout))
            {
                Dialogs.ShowYesNoDialog(Activity, Resource.String.dialog_logout_title, Resource.String.dialog_logout_content, Integration.ClearDataAndStop);
                return true;
            }

            return base.OnPreferenceTreeClick(preference);
        }

        public void OnSharedPreferenceChanged(ISharedPreferences sharedPreferences, string key)
        {
            if (key == GetString(Resource.String.pref_key_advanced_enable_reporting))
            {
                Insights.DisableCollection = !PlatformConfig.Preferences.EnableReporting;
            }
        }

        public override void OnNavigateToScreen(PreferenceScreen preferenceScreen)
        {
            base.OnNavigateToScreen(preferenceScreen);
        }

        public bool OnPreferenceStartScreen(PreferenceFragmentCompat caller, PreferenceScreen pref)
        {
            var args = new Bundle();
            args.PutString(ArgPreferenceRoot, pref.Key);
            var ft = Activity.SupportFragmentManager.BeginTransaction();
            ft.SetTransition(FragmentTransaction.TransitFragmentOpen);
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
