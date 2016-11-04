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
            sp = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
        }

        #region Documents

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

        public bool UnreadIndicatorMe
        {
            get
            {
                return sp.GetBoolean(Application.Context.GetString(Resource.String.pref_key_documents_read_indicator_me), Application.Context.Resources.GetBoolean(Resource.Boolean.pref_documents_read_indicator_me_default));
            }
        }

        public bool LargeAttachmentWarning
        {
            get
            {
                return sp.GetBoolean(Application.Context.GetString(Resource.String.pref_key_documents_large_attachment_warn), Application.Context.Resources.GetBoolean(Resource.Boolean.pref_documents_large_attachment_warn_default));
            }
        }

        public DocumentBodyTypeRequest DocumentBodyRequestType
        {
            get
            {
                return sp.GetBoolean(Application.Context.GetString(Resource.String.pref_key_documents_download_as_plaintext), Application.Context.Resources.GetBoolean(Resource.Boolean.pref_documents_download_as_plaintext_default)) ? DocumentBodyTypeRequest.PlainTextOnly : DocumentBodyTypeRequest.HtmlOnly;
            }
        }

        #endregion

        #region Contacts

        public bool SynchroniseContacts
        {
            get
            {
                return sp.GetBoolean(Application.Context.GetString(Resource.String.pref_key_contacts_synchronised), Application.Context.Resources.GetBoolean(Resource.Boolean.pref_contacts_synchronised_default));
            }
        }

        public bool ContactCommunicationFaxNumbersEnabled
        {
            get
            {
                return sp.GetBoolean(Application.Context.GetString(Resource.String.pref_key_contacts_sub_comm_fax), Application.Context.Resources.GetBoolean(Resource.Boolean.pref_contacts_sub_comm_fax_default));
            }
        }

        public bool ContactCommunicationTelexNumbersEnabled
        {
            get
            {
                return sp.GetBoolean(Application.Context.GetString(Resource.String.pref_key_contacts_sub_comm_telex), Application.Context.Resources.GetBoolean(Resource.Boolean.pref_contacts_sub_comm_telex_default));
            }
        }

        public bool ContactCommunicationImEnabled
        {
            get
            {
                return sp.GetBoolean(Application.Context.GetString(Resource.String.pref_key_contacts_sub_comm_im), Application.Context.Resources.GetBoolean(Resource.Boolean.pref_contacts_sub_comm_im_default));
            }
        }

        public bool ContactCommunicationInternalEnabled
        {
            get
            {
                return sp.GetBoolean(Application.Context.GetString(Resource.String.pref_key_contacts_sub_comm_internal), Application.Context.Resources.GetBoolean(Resource.Boolean.pref_contacts_sub_comm_internal_default));
            }
        }

        public bool ContactCommunicationOtherEnabled
        {
            get
            {
                return sp.GetBoolean(Application.Context.GetString(Resource.String.pref_key_contacts_sub_comm_other), Application.Context.Resources.GetBoolean(Resource.Boolean.pref_contacts_sub_comm_other_default));
            }
        }

        public bool ContactAddressesEnabled
        {
            get
            {
                return sp.GetBoolean(Application.Context.GetString(Resource.String.pref_key_contacts_sub_address), Application.Context.Resources.GetBoolean(Resource.Boolean.pref_contacts_sub_address_default));
            }
        }

        public bool ContactBirthdateEnabled
        {
            get
            {
                return sp.GetBoolean(Application.Context.GetString(Resource.String.pref_key_contacts_sub_birthdate), Application.Context.Resources.GetBoolean(Resource.Boolean.pref_contacts_sub_birthdate_default));
            }
        }

        public bool ContactAccountEnabled
        {
            get
            {
                return sp.GetBoolean(Application.Context.GetString(Resource.String.pref_key_contacts_sub_account), Application.Context.Resources.GetBoolean(Resource.Boolean.pref_contacts_sub_account_default));
            }
        }

        public bool ContactVatEnabled
        {
            get
            {
                return sp.GetBoolean(Application.Context.GetString(Resource.String.pref_key_contacts_sub_vat), Application.Context.Resources.GetBoolean(Resource.Boolean.pref_contacts_sub_vat_default));
            }
        }

        #endregion

        #region Shortcodes


        public bool SynchroniseShortcodes
        {
            get
            {
                return sp.GetBoolean(Application.Context.GetString(Resource.String.pref_key_shortcodes_synchronised), Application.Context.Resources.GetBoolean(Resource.Boolean.pref_shortcodes_synchronised_default));
            }
        }

        #endregion

        #region Composing Documents

        public bool RemoveLine
        {
            get
            {
                return sp.GetBoolean(Application.Context.GetString(Resource.String.pref_key_compose_remove_line), Application.Context.Resources.GetBoolean(Resource.Boolean.pref_compose_remove_line_summary_default));
            }
        }

        public TemplateUsageMode UseTemplate
        {
            get
            {
                return (TemplateUsageMode)int.Parse(sp.GetString(Application.Context.GetString(Resource.String.pref_key_compose_template_mode), Application.Context.GetString(Resource.String.pref_compose_template_mode_default)));
            }
        }

        #endregion

        #region Cache

        public int CleanCacheIntervalDays
        {
            get
            {
                return int.Parse(sp.GetString(Application.Context.GetString(Resource.String.pref_key_cache_auto_clean), Application.Context.Resources.GetString(Resource.String.pref_cache_auto_clean_default)));
            }
        }

        public bool ClearCache
        {
            get
            {
                return sp.GetBoolean(Application.Context.GetString(Resource.String.pref_key_cache_clear), Application.Context.Resources.GetBoolean(Resource.Boolean.pref_cache_clear_default));
            }
            set
            {
                var e = sp.Edit();
                e.PutBoolean(Application.Context.GetString(Resource.String.pref_key_cache_clear), value);
                e.Commit();
            }
        }

        #endregion

        public string PushNotificationToken
        {
            get
            {
                return sp.GetString(Application.Context.GetString(Resource.String.push_notification_token), string.Empty);
            }
            set
            {
                var e = sp.Edit();
                e.PutString(Application.Context.GetString(Resource.String.push_notification_token), value);
                e.Commit();
            }
        }

        public bool EnableReporting
        {
            get
            {
                return sp.GetBoolean(Application.Context.GetString(Resource.String.pref_key_advanced_enable_reporting), Application.Context.Resources.GetBoolean(Resource.Boolean.pref_advanced_enable_reporting_default));
            }
        }

        public enum TemplateUsageMode
        {
            DontUse = 0,
            Default = 1,
            Local = 2,
            AlwaysAsk = 3,
        }
    }
}

