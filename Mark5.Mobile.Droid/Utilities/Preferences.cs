//
// Project: Mark5.Mobile.Droid
// File: Preferences.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Android.App;
using Android.Content;
using Android.Support.V7.Preferences;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Utilities
{

    public class Preferences
    {

        readonly ISharedPreferences sp;

        public Preferences()
        {
            PreferenceManager.SetDefaultValues(Application.Context, Resource.Xml.preferences, true);
            sp = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
        }

        public int DocumentsToDownload
        {
            get
            {
                return int.Parse(sp.GetString(Application.Context.GetString(Resource.String.pref_key_documents_to_load), Application.Context.Resources.GetString(Resource.String.pref_documents_to_load_default)));
            }
        }

        public int MarkAsReadDelaySeconds
        {
            get
            {
                return int.Parse(sp.GetString(Application.Context.GetString(Resource.String.pref_key_documents_mark_as_read), Application.Context.Resources.GetString(Resource.String.pref_documents_mark_as_read_default)));
            }
        }

        public bool LargeAttachmentWarning
        {
            get
            {
                return sp.GetBoolean(Application.Context.GetString(Resource.String.pref_key_documents_large_attachment_warn), Application.Context.Resources.GetBoolean(Resource.Boolean.pref_documents_large_attachment_warn_default));
            }
        }

        public bool DocumentViewPriorityEnabled
        {
            get
            {
                return sp.GetBoolean(Application.Context.GetString(Resource.String.pref_key_documents_sub_priority), Application.Context.Resources.GetBoolean(Resource.Boolean.pref_documents_sub_priority_default));
            }
        }

        public bool DocumentViewReferenceNumberEnabled
        {
            get
            {
                return sp.GetBoolean(Application.Context.GetString(Resource.String.pref_key_documents_sub_reference_number), Application.Context.Resources.GetBoolean(Resource.Boolean.pref_documents_sub_reference_number_default));
            }
        }

        public bool DocumentViewReadByEnabled
        {
            get
            {
                return sp.GetBoolean(Application.Context.GetString(Resource.String.pref_key_documents_sub_read_by), Application.Context.Resources.GetBoolean(Resource.Boolean.pref_documents_sub_read_by_default));
            }
        }

        public bool DocumentViewCreatorEnabled
        {
            get
            {
                return sp.GetBoolean(Application.Context.GetString(Resource.String.pref_key_documents_sub_creator), Application.Context.Resources.GetBoolean(Resource.Boolean.pref_documents_sub_creator_default));
            }
        }

        public bool DocumentViewOriginatorEnabled
        {
            get
            {
                return sp.GetBoolean(Application.Context.GetString(Resource.String.pref_key_documents_sub_originator), Application.Context.Resources.GetBoolean(Resource.Boolean.pref_documents_sub_originator_default));
            }
        }

        public DocumentBodyTypeRequest DocumentBodyRequestType
        {
            get
            {
                return sp.GetBoolean(Application.Context.GetString(Resource.String.pref_key_documents_download_as_plaintext), Application.Context.Resources.GetBoolean(Resource.Boolean.pref_documents_download_as_plaintext_default)) ? DocumentBodyTypeRequest.PlainTextOnly : DocumentBodyTypeRequest.HtmlOnly;
            }
        }

        public bool SynchroniseContacts
        {
            get
            {
                return sp.GetBoolean(Application.Context.GetString(Resource.String.pref_key_contacts_synchronised), Application.Context.Resources.GetBoolean(Resource.Boolean.pref_contacts_synchronised_default));
            }
        }

        public bool SynchroniseShortcodes
        {
            get
            {
                return sp.GetBoolean(Application.Context.GetString(Resource.String.pref_key_shortcodes_synchronised), Application.Context.Resources.GetBoolean(Resource.Boolean.pref_shortcodes_synchronised_default));
            }
        }

        public bool RemoveLine
        {
            get
            {
                return sp.GetBoolean(Application.Context.GetString(Resource.String.pref_key_compose_remove_line), Application.Context.Resources.GetBoolean(Resource.Boolean.pref_compose_remove_line_summary_default));
            }
        }

        public int CleanCacheIntervalDays
        {
            get
            {
                return int.Parse(sp.GetString(Application.Context.GetString(Resource.String.pref_key_cache_auto_clean), Application.Context.Resources.GetString(Resource.String.pref_cache_auto_clean_default)));
            }
        }
    }
}

