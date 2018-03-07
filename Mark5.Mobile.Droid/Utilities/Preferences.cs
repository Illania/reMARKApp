using Android.App;
using Android.Content;
using Android.Support.V7.Preferences;
using Mark5.Mobile.Common.Model;
using System.Collections.Generic;

namespace Mark5.Mobile.Droid.Utilities
{
    public class Preferences
    {
        readonly ISharedPreferences sp;

        public Preferences()
        {
            sp = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
        }

        public IDictionary<string, object> All => sp.All;

        #region Documents

        public bool ShowCreatorOutgoing => sp.GetBoolean(Application.Context.GetString(Resource.String.pref_key_documents_show_creator_outgoing), Application.Context.Resources.GetBoolean(Resource.Boolean.pref_documents_show_creator_outgoing_default));

        public bool UseServerTimeZone => sp.GetBoolean(Application.Context.GetString(Resource.String.pref_key_documents_use_server_timezone), Application.Context.Resources.GetBoolean(Resource.Boolean.pref_documents_use_server_timezone_default));

        public int DocumentsToDownload => int.Parse(sp.GetString(Application.Context.GetString(Resource.String.pref_key_documents_to_load), Application.Context.Resources.GetString(Resource.String.pref_documents_to_load_default)));

        public int MarkAsReadDelaySeconds => int.Parse(sp.GetString(Application.Context.GetString(Resource.String.pref_key_documents_mark_as_read), Application.Context.Resources.GetString(Resource.String.pref_documents_mark_as_read_default)));

        public bool UnreadIndicatorMe => sp.GetBoolean(Application.Context.GetString(Resource.String.pref_key_documents_read_indicator_me), Application.Context.Resources.GetBoolean(Resource.Boolean.pref_documents_read_indicator_me_default));

        public bool CompactDocumentsList => sp.GetBoolean(Application.Context.GetString(Resource.String.pref_key_documents_compact_list), Application.Context.Resources.GetBoolean(Resource.Boolean.pref_documents_compact_list_default));

        public bool LargeAttachmentWarning => sp.GetBoolean(Application.Context.GetString(Resource.String.pref_key_documents_large_attachment_warn), Application.Context.Resources.GetBoolean(Resource.Boolean.pref_documents_large_attachment_warn_default));

        public DocumentBodyTypeRequest DocumentBodyRequestType => sp.GetBoolean(Application.Context.GetString(Resource.String.pref_key_documents_download_as_plaintext), Application.Context.Resources.GetBoolean(Resource.Boolean.pref_documents_download_as_plaintext_default)) ? DocumentBodyTypeRequest.PlainTextOnly : DocumentBodyTypeRequest.HtmlOnly;

        #endregion

        #region Contacts 

        public bool ContactCommunicationFaxNumbersEnabled => sp.GetBoolean(Application.Context.GetString(Resource.String.pref_key_contacts_sub_comm_fax), Application.Context.Resources.GetBoolean(Resource.Boolean.pref_contacts_sub_comm_fax_default));

        public bool ContactCommunicationTelexNumbersEnabled => sp.GetBoolean(Application.Context.GetString(Resource.String.pref_key_contacts_sub_comm_telex), Application.Context.Resources.GetBoolean(Resource.Boolean.pref_contacts_sub_comm_telex_default));

        public bool ContactCommunicationImEnabled => sp.GetBoolean(Application.Context.GetString(Resource.String.pref_key_contacts_sub_comm_im), Application.Context.Resources.GetBoolean(Resource.Boolean.pref_contacts_sub_comm_im_default));

        public bool ContactCommunicationInternalEnabled => sp.GetBoolean(Application.Context.GetString(Resource.String.pref_key_contacts_sub_comm_internal), Application.Context.Resources.GetBoolean(Resource.Boolean.pref_contacts_sub_comm_internal_default));

        public bool ContactCommunicationOtherEnabled => sp.GetBoolean(Application.Context.GetString(Resource.String.pref_key_contacts_sub_comm_other), Application.Context.Resources.GetBoolean(Resource.Boolean.pref_contacts_sub_comm_other_default));

        public bool ContactAddressesEnabled => sp.GetBoolean(Application.Context.GetString(Resource.String.pref_key_contacts_sub_address), Application.Context.Resources.GetBoolean(Resource.Boolean.pref_contacts_sub_address_default));

        public bool ContactBirthdateEnabled => sp.GetBoolean(Application.Context.GetString(Resource.String.pref_key_contacts_sub_birthdate), Application.Context.Resources.GetBoolean(Resource.Boolean.pref_contacts_sub_birthdate_default));

        public bool ContactAccountEnabled => sp.GetBoolean(Application.Context.GetString(Resource.String.pref_key_contacts_sub_account), Application.Context.Resources.GetBoolean(Resource.Boolean.pref_contacts_sub_account_default));

        public bool ContactVatEnabled => sp.GetBoolean(Application.Context.GetString(Resource.String.pref_key_contacts_sub_vat), Application.Context.Resources.GetBoolean(Resource.Boolean.pref_contacts_sub_vat_default));

        #endregion

        #region Composing Documents

        public bool ComposePriorityEnabled => sp.GetBoolean(Application.Context.GetString(Resource.String.pref_key_compose_priority_enabled), Application.Context.Resources.GetBoolean(Resource.Boolean.pref_compose_priority_enabled_default));

        public bool RemoveLine => sp.GetBoolean(Application.Context.GetString(Resource.String.pref_key_compose_remove_line), Application.Context.Resources.GetBoolean(Resource.Boolean.pref_compose_remove_line_default));

        public TemplateUsageMode UseTemplate => (TemplateUsageMode)int.Parse(sp.GetString(Application.Context.GetString(Resource.String.pref_key_compose_template_mode), Application.Context.GetString(Resource.String.pref_compose_template_mode_default)));

        public string LocalTemplate => sp.GetString(Application.Context.GetString(Resource.String.pref_key_compose_template_local), Application.Context.GetString(Resource.String.pref_compose_template_local_default));

        public bool AlwaysUseDefaultLine => sp.GetBoolean(Application.Context.GetString(Resource.String.pref_key_always_use_default_line), Application.Context.Resources.GetBoolean(Resource.Boolean.pref_always_use_default_line_default));

        #endregion

        #region Search

        public int MaxDocumentsToSearch => int.Parse(sp.GetString(Application.Context.GetString(Resource.String.pref_key_search_documents_to_get), Application.Context.Resources.GetString(Resource.String.pref_search_documents_to_get_default)));

        public int MaxContactsToSearch => int.Parse(sp.GetString(Application.Context.GetString(Resource.String.pref_key_search_contacts_to_get), Application.Context.Resources.GetString(Resource.String.pref_search_contacts_to_get_default)));

        public int MaxShortcodesToSearch => int.Parse(sp.GetString(Application.Context.GetString(Resource.String.pref_key_search_shortcodes_to_get), Application.Context.Resources.GetString(Resource.String.pref_search_shortcodes_to_get_default)));

        public bool PartialWordSearch => sp.GetBoolean(Application.Context.GetString(Resource.String.pref_key_search_partial_word), Application.Context.Resources.GetBoolean(Resource.Boolean.pref_search_partial_word_default));

        #endregion

        #region Security

        public int FingerPrintAuthInterval => int.Parse(sp.GetString(Application.Context.GetString(Resource.String.pref_key_fingerprint_auth), Application.Context.Resources.GetString(Resource.String.pref_fingerprint_auth_default)));

        public bool FingerPrintAuthEnabled => FingerPrintAuthInterval > int.Parse(Application.Context.Resources.GetString(Resource.String.pref_fingerprint_auth_default));

        #endregion

        public bool HideReadNotifications => sp.GetBoolean(Application.Context.GetString(Resource.String.pref_key_notification_hide_read), Application.Context.Resources.GetBoolean(Resource.Boolean.pref_notification_hide_read_default));

        public bool SilenceNotifications => sp.GetBoolean(Application.Context.GetString(Resource.String.pref_key_notification_silence), Application.Context.Resources.GetBoolean(Resource.Boolean.pref_notification_silence_default));

        public string NotificationsRingtone
        {
            get => sp.GetString(Application.Context.GetString(Resource.String.pref_key_notification_ringtone), Application.Context.Resources.GetString(Resource.String.pref_notification_ringtone_default));
            set
            {
                var e = sp.Edit();
                e.PutString(Application.Context.GetString(Resource.String.pref_key_notification_ringtone), value);
                e.Commit();
            }
        }

        public bool NotificationsVibrate => sp.GetBoolean(Application.Context.GetString(Resource.String.pref_key_notification_vibrate), Application.Context.Resources.GetBoolean(Resource.Boolean.pref_notification_vibrate_default));

        public int CleanCacheIntervalDays => int.Parse(sp.GetString(Application.Context.GetString(Resource.String.pref_key_cache_auto_clean), Application.Context.Resources.GetString(Resource.String.pref_cache_auto_clean_default)));

        public bool ClearCache
        {
            get => sp.GetBoolean(Application.Context.GetString(Resource.String.pref_key_cache_clear), Application.Context.Resources.GetBoolean(Resource.Boolean.pref_cache_clear_default));
            set
            {
                var e = sp.Edit();
                e.PutBoolean(Application.Context.GetString(Resource.String.pref_key_cache_clear), value);
                e.Commit();
            }
        }

        public bool EnableReporting => sp.GetBoolean(Application.Context.GetString(Resource.String.pref_key_advanced_enable_reporting), Application.Context.Resources.GetBoolean(Resource.Boolean.pref_advanced_enable_reporting_default));

        public string PushNotificationToken
        {
            get => sp.GetString(Application.Context.GetString(Resource.String.push_notification_token), string.Empty);
            set
            {
                var e = sp.Edit();
                e.PutString(Application.Context.GetString(Resource.String.push_notification_token), value);
                e.Commit();
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