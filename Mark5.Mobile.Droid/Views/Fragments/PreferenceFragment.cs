//
// Project: Mark5.Mobile.Droid
// File: PreferenceFragment.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Android.OS;
using Android.Support.V7.Preferences;
using AFollestad.MaterialDialogs;
using Java.Lang;

namespace Mark5.Mobile.Droid.Views.Fragments
{

    public class PreferenceFragment : PreferenceFragmentCompat
    {

        const string PrefKeyDocumentsViewOptions = "pref_key_documents_view_options";
        const string PrefKeyContactsViewOptions = "pref_key_contacts_view_options";

        public override void OnCreatePreferences(Bundle savedInstanceState, string rootKey)
        {
            AddPreferencesFromResource(Resource.Xml.preferences);
        }

        public override bool OnPreferenceTreeClick(Preference preference)
        {
            var handled = base.OnPreferenceTreeClick(preference);

            if (!handled && preference.Key == FindPreference(PrefKeyDocumentsViewOptions)?.Key)
            {


                handled = true;
            }
            if (!handled && preference.Key == FindPreference(PrefKeyContactsViewOptions)?.Key)
            {


                handled = true;
            }

            return handled;
        }

        class MultiChoiceCallback : Java.Lang.Object, MaterialDialog.IListCallbackMultiChoice
        {

            public bool OnSelection(MaterialDialog p0, Integer[] p1, ICharSequence[] p2)
            {
                return true;
            }
        }
    }
}
