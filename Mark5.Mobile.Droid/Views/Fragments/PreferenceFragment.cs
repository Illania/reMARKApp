//
// Project: Mark5.Mobile.Droid
// File: PreferenceFragment.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V7.App;
using Android.Support.V7.Preferences;
using Mark5.Mobile.Common;

namespace Mark5.Mobile.Droid.Views.Fragments
{

    public class PreferenceFragment : PreferenceFragmentCompat, PreferenceFragmentCompat.IOnPreferenceStartScreenCallback
    {

        const string PrefKeyAboutVersion = "pref_key_about_version";

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

            ((AppCompatActivity)Activity).SupportActionBar.Title = PreferenceScreen.Title; ;
        }

        public override void OnCreatePreferences(Bundle savedInstanceState, string rootKey)
        {
            SetPreferencesFromResource(Resource.Xml.preferences, rootKey);

            var versionPreference = FindPreference(PrefKeyAboutVersion);
            if (versionPreference != null)
            {
                versionPreference.Summary = CommonConfig.DeviceInfoProvider.GetAppVersionString();
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
