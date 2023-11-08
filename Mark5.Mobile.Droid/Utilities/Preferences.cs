using Android.App;
using Android.Content;
using AndroidX.Preference;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mark5.Mobile.Droid.Utilities
{
    public class Preferences
    {
        ISharedPreferences sp => PreferenceManager.GetDefaultSharedPreferences(Application.Context);

        public IDictionary<string, object> All => sp.All;

        #region Documents

        public bool ShowCreatorOutgoing => sp.GetBoolean(Application.Context.GetString(Resource.String.pref_key_documents_show_creator_outgoing), Application.Context.Resources.GetBoolean(Resource.Boolean.pref_documents_show_creator_outgoing_default));

        public bool UseServerTimeZone => sp.GetBoolean(Application.Context.GetString(Resource.String.pref_key_documents_use_server_timezone), Application.Context.Resources.GetBoolean(Resource.Boolean.pref_documents_use_server_timezone_default));

        public int DocumentsToDownload => int.Parse(sp.GetString(Application.Context.GetString(Resource.String.pref_key_documents_to_load), Application.Context.Resources.GetString(Resource.String.pref_documents_to_load_default)));

        public int MarkAsReadDelaySeconds => int.Parse(sp.GetString(Application.Context.GetString(Resource.String.pref_key_documents_mark_as_read), Application.Context.Resources.GetString(Resource.String.pref_documents_mark_as_read_default)));

        public bool UnreadIndicatorMe => sp.GetBoolean(Application.Context.GetString(Resource.String.pref_key_documents_read_indicator_me), Application.Context.Resources.GetBoolean(Resource.Boolean.pref_documents_read_indicator_me_default));

        public bool SortByDate => sp.GetBoolean(Application.Context.GetString(Resource.String.pref_key_documents_sort_by_date), Application.Context.Resources.GetBoolean(Resource.Boolean.pref_documents_sort_by_date_default));

        public bool ShowTimeOlderEmails => sp.GetBoolean(Application.Context.GetString(Resource.String.pref_key_documents_show_time_older_emails), Application.Context.Resources.GetBoolean(Resource.Boolean.pref_documents_show_time_older_emails_default));

        public bool CompactDocumentsList => sp.GetBoolean(Application.Context.GetString(Resource.String.pref_key_documents_compact_list), Application.Context.Resources.GetBoolean(Resource.Boolean.pref_documents_compact_list_default));

        public bool LargeAttachmentWarning => sp.GetBoolean(Application.Context.GetString(Resource.String.pref_key_documents_large_attachment_warn), Application.Context.Resources.GetBoolean(Resource.Boolean.pref_documents_large_attachment_warn_default));

        public bool EnableMoveToFolder => sp.GetBoolean(Application.Context.GetString(Resource.String.pref_key_documents_enable_move_to_folder), Application.Context.Resources.GetBoolean(Resource.Boolean.pref_documents_enable_move_to_folder_default));

        public DocumentBodyTypeRequest DocumentBodyRequestType => sp.GetBoolean(Application.Context.GetString(Resource.String.pref_key_documents_download_as_plaintext), Application.Context.Resources.GetBoolean(Resource.Boolean.pref_documents_download_as_plaintext_default)) ? DocumentBodyTypeRequest.PlainTextOnly : DocumentBodyTypeRequest.HtmlOnly;

        public bool UseMessageListAppearance => sp.GetBoolean(Application.Context.GetString(Resource.String.pref_key_use_message_list_appearance), Application.Context.Resources.GetBoolean(Resource.Boolean.pref_use_message_list_appearance_default));

        public bool ConfirmationRemoveSwipe => sp.GetBoolean(Application.Context.GetString(Resource.String.pref_key_documents_confirm_remove), Application.Context.Resources.GetBoolean(Resource.Boolean.pref_documents_confirm_remove_default));

        public int SendingDelay => int.Parse(sp.GetString(Application.Context.GetString(Resource.String.pref_key_sending_delay), Application.Context.Resources.GetString(Resource.String.pref_sending_delay_default)));

        public bool RememberLastUserDelaySettings => sp.GetBoolean(Application.Context.GetString(Resource.String.pref_key_remember_last_user_delay_settings), Application.Context.Resources.GetBoolean(Resource.Boolean.pref_remember_last_user_delay_settings_default));

        public bool ReplyWithAttachments => sp.GetBoolean(Application.Context.GetString(Resource.String.pref_key_reply_with_attachments), Application.Context.Resources.GetBoolean(Resource.Boolean.pref_reply_with_attachments_default));

        public bool OpenFileToFolderDialog => sp.GetBoolean(Application.Context.GetString(Resource.String.pref_key_open_file_to_folder_dialog), Application.Context.Resources.GetBoolean(Resource.Boolean.pref_open_file_to_folder_dialog_default));

        public bool SyncUserActivities => sp.GetBoolean(Application.Context.GetString(Resource.String.pref_key_sync_user_activities), Application.Context.Resources.GetBoolean(Resource.Boolean.pref_sync_user_activities_default));

        #endregion

        #region Caller Identification

        public bool CallerIdentificationEnabled
        {
            get => sp.GetBoolean(Application.Context.GetString(Resource.String.pref_key_callidentification_identification_enabled), Application.Context.Resources.GetBoolean(Resource.Boolean.pref_callidentification_enabled_default));
            set
            {
                var e = sp.Edit();
                e.PutBoolean(Application.Context.GetString(Resource.String.pref_key_callidentification_identification_enabled), value);
                e.Commit();
            }
        }

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

        public int AuthorizationInterval => int.Parse(sp.GetString(Application.Context.GetString(Resource.String.pref_key_auth), Application.Context.Resources.GetString(Resource.String.pref_auth_default)));

        public bool AuthorizationEnabled => AuthorizationInterval > int.Parse(Application.Context.Resources.GetString(Resource.String.pref_auth_default));

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

        public string AzureApplicationProxyBearerToken
        {
            get => sp.GetString(Application.Context.GetString(Resource.String.azure_application_proxy_token), string.Empty);
            set
            {
                var e = sp.Edit();
                e.PutString(Application.Context.GetString(Resource.String.azure_application_proxy_token), value);
                e.Commit();
            }
        }



        public int PresetCategoryId
        {

            get => sp.GetInt(Application.Context.GetString(Resource.String.pref_key_set_preset_category), 0);
            set
            {
                var e = sp.Edit();
                e.PutInt(Application.Context.GetString(Resource.String.pref_key_set_preset_category), value);
                e.Commit();
            }
        }
           

        public string AzureApplicationProxyAppClientId
        {
            get => sp.GetString(Application.Context.GetString(Resource.String.azure_application_proxy_app_client_id), string.Empty);
            set
            {
                var e = sp.Edit();
                e.PutString(Application.Context.GetString(Resource.String.azure_application_proxy_app_client_id), value);
                e.Commit();
            }
        }

        public string AzureApplicationProxyAppProxyId
        {
            get => sp.GetString(Application.Context.GetString(Resource.String.azure_application_proxy_app_proxy_id), string.Empty);
            set
            {
                var e = sp.Edit();
                e.PutString(Application.Context.GetString(Resource.String.azure_application_proxy_app_proxy_id), value);
                e.Commit();
            }
        }

        public bool AzureApplicationProxyEnabled
        {
            get => sp.GetBoolean(Application.Context.GetString(Resource.String.azure_application_proxy_enabled), false);
            set
            {
                var e = sp.Edit();
                e.PutBoolean(Application.Context.GetString(Resource.String.azure_application_proxy_enabled), value);
                e.Commit();
            }
        }


        public int LastUserSendingDelay
        {
            get => sp.GetInt(Application.Context.GetString(Resource.String.pref_key_last_user_sending_delay), 0);
            set
            {
                var e = sp.Edit();
                e.PutInt(Application.Context.GetString(Resource.String.pref_key_last_user_sending_delay), value);
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

        public const int MarkAsRead = 10;
        public const int MarkAsUnread = 11;
        public const int CopyToWorktray = 20;
        public const int CopyToFolder = 30;
        public const int MoveToFolder = 31;
        public const int SetPriority = 40;
        public const int Categories = 50;
        public const int DeleteFromFolder = 60;
        public const int Delete = 61;

        #region Email swipe actions related

        public enum EmailSwipeAction
        {
            MarkAsReadUnread = 1,
            MoveToFolder = 2,
            CopyToWorkTray = 3,
            CopyToFolder = 4,
            Categories = 5,
            Priorities = 6,
            RemoveFromFolder = 7,
            Delete = 8,
            PresetCategory = 9,
            AddBookmark = 10,
            More = 11
        }

        public EmailSwipeAction EmailLeadingSwipeAction
        {

            get
            {
                var pref = sp.GetString(Application.Context.GetString(Resource.String.pref_key_swipe_leading), Application.Context.Resources.GetString(Resource.String.pref_email_swipe_actions_leading_default));
                return (EmailSwipeAction)Enum.Parse(typeof(EmailSwipeAction), pref);
            }

            set
            {
                var e = sp.Edit();
                e.PutString(Application.Context.GetString(Resource.String.pref_key_swipe_leading), $"{(int)value}");
                e.Commit();
            }
        }

        public EmailSwipeAction EmailTrailingSwipeAction
        {
            get
            {
                var pref = sp.GetString(Application.Context.GetString(Resource.String.pref_key_swipe_trailing), Application.Context.Resources.GetString(Resource.String.pref_email_swipe_actions_trailing_default));
                return (EmailSwipeAction)Enum.Parse(typeof(EmailSwipeAction), pref);
            }

            set
            {
                var e = sp.Edit();
                e.PutString(Application.Context.GetString(Resource.String.pref_key_swipe_trailing), $"{(int)value}");
                e.Commit();
            }
        }
       
        public List<EmailSwipeAction> GetAllAvailableActions()
        {
            var arr = Application.Context.Resources.GetStringArray(Resource.Array.pref_email_swipe_actions_entryvalues).ToList();
            var selectArr = arr.Select(x => (EmailSwipeAction)Enum.Parse(typeof(EmailSwipeAction), x));
            return selectArr.ToList();
        }

        public List<EmailSwipeAction> GetEnabledActions()
        {
            var allAvailableActions = GetAllAvailableActions();
            if (allAvailableActions.Contains(EmailSwipeAction.MoveToFolder) && !PlatformConfig.Preferences.EnableMoveToFolder)
                allAvailableActions.Remove(EmailSwipeAction.MoveToFolder);
            return allAvailableActions;
        }

        public List<EmailSwipeAction> GetAvailableSwipeActions()
        {
            var exceptLeading = GetAllAvailableActions().Where(x =>
            {
                return x != EmailLeadingSwipeAction;
            });
            var exceptTrailing = exceptLeading.Where(x =>
            {
                return x != EmailTrailingSwipeAction;
            });
            return exceptTrailing.ToList();
        }

        public void ResetSwipeActions()
        {
            EmailLeadingSwipeAction = EmailSwipeAction.Categories;
            EmailTrailingSwipeAction = EmailSwipeAction.CopyToWorkTray;
        }

        #endregion

        #region Bookmarks

        public Dictionary<int, int> BookmarksForFolders
        {
            get
            {
                var pref = sp.GetStringSet(Application.Context.GetString(Resource.String.pref_key_bookmarks), new List<string>());
                var dict = new Dictionary<int, int>();
                foreach (var keyValue in pref)
                {
                    var splitString = keyValue.Split(":");
                    dict.Add(Convert.ToInt32(splitString[0]), Convert.ToInt32(splitString[1]));
                }
                return dict;
            }

            set
            {
                var e = sp.Edit();
                var bookmarkList = new List<string>();
                foreach (var keyValue in value)
                {
                    bookmarkList.Add(string.Join(":", new List<string> { keyValue.Key.ToString(), keyValue.Value.ToString() }));
                }
                e.PutStringSet(Application.Context.GetString(Resource.String.pref_key_bookmarks), bookmarkList);
                e.Commit();
            }
        }

        public bool HasBookmarkForFolder(int folderId, int docId)
        {
            var hasBookmark = BookmarksForFolders.Contains(new KeyValuePair<int, int>(folderId, docId));
            return hasBookmark;
        }

        public void SetBookmarkForFolder(int folderId, int docId)
        {
            var newBookmarks = BookmarksForFolders;
            if (!BookmarksForFolders.ContainsKey(folderId))
                newBookmarks.Add(folderId, docId);
            else
                newBookmarks[folderId] = docId;

            BookmarksForFolders = newBookmarks;
        }

        public int GetBookmarkForFolder(int folderId)
        {
            if (!BookmarksForFolders.ContainsKey(folderId))
                return -1;
            else
                return Convert.ToInt32(BookmarksForFolders[folderId]);
        }

        public void RemoveBookmarkForFolder(int folderId)
        {
            var newBookmarks = BookmarksForFolders;
            if (BookmarksForFolders.ContainsKey(folderId))
                newBookmarks.Remove(folderId);

            BookmarksForFolders = newBookmarks;
        }

        #endregion

        #region ExtraFieldsSettings

        public Dictionary<int, bool> ExtraFieldsSettings
        {
            get
            {
                var pref = sp.GetStringSet(Application.Context.GetString(Resource.String.pref_key_extra_fields_settings), new List<string>());
                var dict = new Dictionary<int, bool>();
                foreach (var keyValue in pref)
                {
                    var splitString = keyValue.Split(":");
                    dict.Add(Convert.ToInt32(splitString[0]), Convert.ToBoolean(splitString[1]));
                }
                return dict;
            }

            set
            {
                var e = sp.Edit();
                var extraFieldsSettingsList = new List<string>();
                foreach (var keyValue in value)
                {
                    extraFieldsSettingsList.Add(string.Join(":", new List<string> { keyValue.Key.ToString(), keyValue.Value.ToString() }));
                }
                e.PutStringSet(Application.Context.GetString(Resource.String.pref_key_extra_fields_settings), extraFieldsSettingsList);
                e.Commit();
            }
        }

        public bool IsExtraFieldEnabled(int extraFieldId)
        {
            if (!ExtraFieldsSettings.ContainsKey(extraFieldId))
                return false;
            else
                return Convert.ToBoolean(ExtraFieldsSettings[extraFieldId]);
        }

        public void SetExtraFieldEnabled(int extraFieldId, bool enabled)
        {
            var newExtraFieldSettings = ExtraFieldsSettings;
            if (!ExtraFieldsSettings.ContainsKey(extraFieldId))
               newExtraFieldSettings.Add(extraFieldId, enabled);
            else
                newExtraFieldSettings[extraFieldId] = enabled;

            ExtraFieldsSettings = newExtraFieldSettings;
        }
        #endregion


        #region Recent Address swipe actions
        public enum RecentAddressSwipeAction
        {         
            Delete = 1
        }
        #endregion

        #region Categories swipe actions
        public enum CategoriesSwipeAction
        {
            AddToFavorites,
            RemoveFromFavorites
        }
        #endregion

        #region Favorite folders

        public FavoriteFoldersSyncType SyncFavoriteFolders
        {
            get
            {
                if (!ServerConfig.SystemSettings.SystemInfo.SyncFavoritesWithDesktopAvailable)
                {
                    return (FavoriteFoldersSyncType)int.Parse(sp.GetString(Application.Context.GetString(Resource.String.pref_key_sync_favorite_folders_old),
                        Application.Context.GetString(Resource.String.pref_sync_favorite_folders_default)));
                }
                else
                {
                    return (FavoriteFoldersSyncType)int.Parse(sp.GetString(Application.Context.GetString(Resource.String.pref_key_sync_favorite_folders),
                        Application.Context.GetString(Resource.String.pref_sync_favorite_folders_default)));
                }
            }
            set
            {
                var e = sp.Edit();
                if (!ServerConfig.SystemSettings.SystemInfo.SyncFavoritesWithDesktopAvailable)
                {
                    e.PutString(Application.Context.GetString(Resource.String.pref_key_sync_favorite_folders_old), $"{(int)value}");
                }
                else
                {
                    e.PutString(Application.Context.GetString(Resource.String.pref_key_sync_favorite_folders), $"{(int)value}");
                }
                e.Commit();
            }
        }



        #endregion
    }
}