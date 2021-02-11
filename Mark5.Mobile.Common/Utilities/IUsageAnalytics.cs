using System.Collections.Generic;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Common.Utilities
{
    public interface IUsageAnalytics
    {
        void LogEvent(AnalyticsEvent analyticsEvent);
        void SetUserProperty(UserProperty property, string value);
        void SetScreen(string screenClass);
    }

    #region Enums

    public enum UserProperty
    {
        SSL,
        Hostname,
        CustomerName,
    }

    public enum ContactActionChoice
    {
        Email,
        Call,
        Text,
        Map
    }

    public enum ContactPickerChoice
    {
        Recents,
        Contacts,
        Internal,
        Shortcodes,
        Phonebook,
    }

    public enum AddAttachmentType
    {
        TakePhoto,
        PickPhoto,
        Local
    }

    public enum TemplateType
    {
        Local,
        Default,
        Another
    }

    #endregion

    #region Abstract

    public abstract class AnalyticsEvent
    {
        public string EventName { get; }

        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();

        protected AnalyticsEvent(string eventName)
        {
            EventName = eventName;
        }

        protected AnalyticsEvent(ModuleType module, string eventName)
        {
            EventName = GetModuleString(module) + "_" + eventName;
        }

        protected AnalyticsEvent(ModuleType module, string eventName, int quantity)
        {
            string quantityString;
            if (quantity < 1)
                quantityString = "zero";
            else if (quantity < 2)
                quantityString = "one";
            else
                quantityString = "many";

            EventName = GetModuleString(module) + "_" + eventName + "_" + quantityString;
        }

        string GetModuleString(ModuleType module)
        {
            return module.ToString().ToLowerInvariant().SafeSubstring(0, 3);
        }
    }

    #endregion

    #region Common events

    public class PullToRefreshEvent : AnalyticsEvent
    {
        public PullToRefreshEvent(bool folder, ModuleType module)
            : base(module, "pull_to_refresh" + (folder ? "_folder" : ""))
        {
        }
    }

    public class FilterEvent : AnalyticsEvent
    {
        public FilterEvent(bool folder, ModuleType module)
            : base(module, "filter" + (folder ? "_folders" : ""))
        {
        }
    }

    internal class CopyToWorktrayEvent : AnalyticsEvent
    {
        public CopyToWorktrayEvent(ModuleType module, int quantity)
            : base(module, "copy_to_worktray", quantity)
        {
        }
    }

    internal class CopyToUserWorktrayEvent : AnalyticsEvent
    {
        public CopyToUserWorktrayEvent(ModuleType module, int quantity)
            : base(module, "copy_to_user_worktray", quantity)
        {
        }
    }

    internal class CopyToFolderEvent : AnalyticsEvent
    {
        public CopyToFolderEvent(ModuleType module, int quantity)
            : base(module, "copy_to_folder", quantity)
        {
        }
    }

    internal class MoveToFolderEvent : AnalyticsEvent
    {
        public MoveToFolderEvent(ModuleType module, int quantity)
            : base(module, "move_to_folder", quantity)
        {
        }
    }

    internal class DeleteEvent : AnalyticsEvent
    {
        public DeleteEvent(ModuleType module, int quantity)
            : base(module, "delete", quantity)
        {
        }
    }

    internal class DeleteFromFolderEvent : AnalyticsEvent
    {
        public DeleteFromFolderEvent(ModuleType module, int quantity)
            : base(module, "delete_from_folder", quantity)
        {
        }
    }

    internal class SetCategoriesEvent : AnalyticsEvent
    {
        public SetCategoriesEvent(ModuleType module, int quantity)
            : base(module, "set_categories", quantity)
        {
        }
    }

    internal class AddCommentEvent : AnalyticsEvent
    {
        public AddCommentEvent(ModuleType module)
            : base(module, "add_comment")
        {
        }
    }

    internal class DeleteCommentEvent : AnalyticsEvent
    {
        public DeleteCommentEvent(ModuleType module)
            : base(module, "delete_comment")
        {
        }
    }

    #endregion

    #region Folder events

    public class OpenFolderEvent : AnalyticsEvent
    {
        public OpenFolderEvent(ModuleType module, bool isFromFavourite = false)
            : base(module, "open_folder" + (isFromFavourite ? "_favourite" : ""))
        {
        }
    }

    public class OpenOutgoingFolderEvent : AnalyticsEvent
    {
        public OpenOutgoingFolderEvent()
            : base(ModuleType.Documents, "open_outgoing")
        {
        }
    }

    public class ExpandFolderEvent : AnalyticsEvent
    {
        public ExpandFolderEvent(ModuleType module)
            : base(module, "expand_folder")
        {
        }
    }

    public class SetFolderFavoriteEvent : AnalyticsEvent
    {
        public SetFolderFavoriteEvent(ModuleType module, int quantity)
            : base(module, "set_favorite", quantity)
        {
        }
    }

    public class SetFolderSyncEvent : AnalyticsEvent
    {
        public SetFolderSyncEvent(ModuleType module, int quantity)
            : base(module, "set_sync", quantity)
        {
        }
    }

    public class SetFolderNotifyEvent : AnalyticsEvent
    {
        public SetFolderNotifyEvent(ModuleType module, int quantity)
            : base(module, "set_notify", quantity)
        {
        }
    }

    #endregion

    #region Document events

    public class DocumentQuickSwitchEvent : AnalyticsEvent
    {
        public DocumentQuickSwitchEvent()
            : base(ModuleType.Documents, "quick_switch")
        {
        }
    }

    public class DocumentOpenAttachmentEvent : AnalyticsEvent
    {
        public DocumentOpenAttachmentEvent()
            : base(ModuleType.Documents, "open_attachment")
        {
        }
    }

    public class SetReadStatusEvent : AnalyticsEvent
    {
        public SetReadStatusEvent(int quantity)
            : base(ModuleType.Documents, "set_read_status", quantity)
        {
        }
    }

    public class CancelSendEvent : AnalyticsEvent
    {
        public CancelSendEvent()
            : base(ModuleType.Documents, "cancel_send")
        {
        }
    }

    public class ForceSendEvent : AnalyticsEvent
    {
        public ForceSendEvent()
            : base(ModuleType.Documents, "force_send")
        {
        }
    }

    public class SetPriorityEvent : AnalyticsEvent
    {
        public SetPriorityEvent(int quantity)
            : base(ModuleType.Documents, "set_priority", quantity)
        {
        }
    }

    public class GetMoreDocumentsEvent : AnalyticsEvent
    {
        public GetMoreDocumentsEvent()
            : base(ModuleType.Documents, "get_more")
        {
        }
    }

    public class ComposeAddAttachmentEvent : AnalyticsEvent
    {
        public ComposeAddAttachmentEvent(AddAttachmentType type)
            : base(ModuleType.Documents, "compose_add_attachment_" + type.ToString().ToLowerInvariant())
        {
        }
    }

    public class ComposeRemoveAttachmentEvent : AnalyticsEvent
    {
        public ComposeRemoveAttachmentEvent()
            : base(ModuleType.Documents, "compose_remove_attachment")
        {
        }
    }

    public class ComposeOpenAttachmentEvent : AnalyticsEvent
    {
        public ComposeOpenAttachmentEvent()
            : base(ModuleType.Documents, "compose_open_attachment")
        {
        }
    }

    public class ComposeShowPreviousEmailEvent : AnalyticsEvent
    {
        public ComposeShowPreviousEmailEvent()
            : base(ModuleType.Documents, "compose_show_previous")
        {
        }
    }

    public class ComposeEditedPreviousEmailEvent : AnalyticsEvent
    {
        public ComposeEditedPreviousEmailEvent()
            : base(ModuleType.Documents, "compose_edited_previous")
        {
        }
    }

    public class ComposeSaveDraftEvent : AnalyticsEvent
    {
        public ComposeSaveDraftEvent()
            : base(ModuleType.Documents, "compose_save_draft")
        {
        }
    }

    public class ComposeInsertTemplateEvent : AnalyticsEvent
    {
        public ComposeInsertTemplateEvent()
            : base(ModuleType.Documents, "compose_insert_template")
        {
        }
    }

    public class ComposeAddTemplateEvent : AnalyticsEvent
    {
        public ComposeAddTemplateEvent(TemplateType? type)
            : base(ModuleType.Documents, "compose_add_template")
        {
            var parameterString = type == null ? "null" : type.ToString().ToLowerInvariant();
            Parameters.Add("type", parameterString);
        }
    }

    public class ComposeContactPickerEvent : AnalyticsEvent
    {
        public ComposeContactPickerEvent(ContactPickerChoice type)
            : base(ModuleType.Documents, "compose_picker")
        {
            Parameters.Add("choice", type.ToString().ToLowerInvariant());
        }
    }

    public class DocumentRecoveredEvent : AnalyticsEvent
    {
        public DocumentRecoveredEvent(bool continueEditing)
            : base(ModuleType.Documents, "recovered" + (continueEditing ? "_editing" : ""))
        {
        }
    }

    internal class DocumentSentEvent : AnalyticsEvent
    {
        public DocumentSentEvent(DocumentCreationModeFlag flag)
            : base(ModuleType.Documents, "sent")
        {
            Parameters.Add("creation_flag", flag.ToString().ToLowerInvariant());
        }
    }

    #endregion

    #region Compose opening events

    public class ComposeEvent : AnalyticsEvent
    {
        protected ComposeEvent(string mode)
            : base(ModuleType.Documents, "compose")
        {
            Parameters.Add("mode", mode);
        }
    }

    public class ComposeReplyEvent : ComposeEvent
    {
        public ComposeReplyEvent()
            : base("reply")
        {
        }
    }

    public class ComposeReplyAllEvent : ComposeEvent
    {
        public ComposeReplyAllEvent()
            : base("reply_all")
        {
        }
    }

    public class ComposeForwardEvent : ComposeEvent
    {
        public ComposeForwardEvent()
            : base("forward")
        {
        }
    }

    public class ComposeCopyToNewEvent : ComposeEvent
    {
        public ComposeCopyToNewEvent()
            : base("copy_to_new")
        {
        }
    }

    public class ComposeNewDocumentEvent : ComposeEvent
    {
        public ComposeNewDocumentEvent()
            : base("new")
        {
        }
    }

    public class ComposeEditDraftEvent : ComposeEvent
    {
        public ComposeEditDraftEvent()
            : base("edit_draft")
        {
        }
    }

    #endregion

    #region Contact events

    public class ContactFastActionEvent : AnalyticsEvent
    {
        public ContactFastActionEvent(ContactActionChoice type)
            : base(ModuleType.Contacts, "fast_action_" + type.ToString().ToLowerInvariant())
        {
        }
    }

    public class ContactActionEvent : AnalyticsEvent
    {
        public ContactActionEvent(ContactActionChoice type)
            : base(ModuleType.Contacts, "action_" + type.ToString().ToLowerInvariant())
        {
        }
    }

    public class ContactNavigateSubContactEvent : AnalyticsEvent
    {
        public ContactNavigateSubContactEvent()
            : base(ModuleType.Contacts, "navigate_subcontact")
        {
        }
    }

    internal class AddContactEvent : AnalyticsEvent
    {
        public AddContactEvent()
            : base(ModuleType.Contacts, "add")
        {
        }
    }

    internal class EditContactEvent : AnalyticsEvent
    {
        public EditContactEvent()
            : base(ModuleType.Contacts, "edit")
        {
        }
    }

    internal class AddSubContactEvent : AnalyticsEvent
    {
        public AddSubContactEvent()
            : base(ModuleType.Contacts, "add_subcontact")
        {
        }
    }

    #endregion

    #region Shortcode events

    public class ShortcodeClickEmailEvent : AnalyticsEvent
    {
        public ShortcodeClickEmailEvent()
            : base(ModuleType.Shortcodes, "click_email")
        {
        }
    }

    public class ShortcodeComposeDocumentEvent : AnalyticsEvent
    {
        public ShortcodeComposeDocumentEvent()
            : base(ModuleType.Shortcodes, "compose_document")
        {
        }
    }

    internal class AddShortcodeEvent : AnalyticsEvent
    {
        public AddShortcodeEvent()
            : base(ModuleType.Shortcodes, "add")
        {
        }
    }

    internal class EditShortcodeEvent : AnalyticsEvent
    {
        public EditShortcodeEvent()
            : base(ModuleType.Shortcodes, "edit")
        {
        }
    }

    #endregion

    #region Notification events

    public class NotificationClickedEvent : AnalyticsEvent
    {
        public NotificationClickedEvent(ModuleType module)
            : base(module, "notification_clicked")
        {
        }
    }

    public class NotificationMarkAllAsReadEvent : AnalyticsEvent
    {
        public NotificationMarkAllAsReadEvent(ModuleType module)
            : base(module, "notification_mark_all_as_read")
        {
        }
    }

    #endregion

    #region Setting events

    public class SettingsUpdateSystemConfigurationEvent : AnalyticsEvent
    {
        public SettingsUpdateSystemConfigurationEvent()
            : base("settings_update_system_configuration")
        {
        }
    }

    public class SettingsCacheCleanUpEvent : AnalyticsEvent
    {
        public SettingsCacheCleanUpEvent()
            : base("settings_cache_clean_up")
        {
        }
    }

    public class SettingsLogOutEvent : AnalyticsEvent
    {
        public SettingsLogOutEvent()
            : base("settings_log_out")
        {
        }
    }

    #endregion

    #region Search

    public class DoSearchEvent : AnalyticsEvent
    {
        public DoSearchEvent(ModuleType module)
            : base(module, "search")
        {
        }
    }

    #endregion

    #region View Opening events

    public class OpenModuleEvent : AnalyticsEvent
    {
        public OpenModuleEvent(ModuleType module)
            : base(module, "module_open")
        {
        }
    }
    public class OpenDocumentEvent : AnalyticsEvent
    {
        public OpenDocumentEvent(bool external)
            : base(ModuleType.Documents, "open" + (external ? "_external" : ""))
        {
        }
    }

    public class OpenContactEvent : AnalyticsEvent
    {
        public OpenContactEvent()
            : base(ModuleType.Contacts, "open")
        {
        }
    }

    public class OpenShortcodeEvent : AnalyticsEvent
    {
        public OpenShortcodeEvent()
            : base(ModuleType.Shortcodes, "open")
        {
        }
    }

    public class OpenLinksEvent : AnalyticsEvent
    {
        public OpenLinksEvent(ModuleType module)
            : base(module, "links_open")
        {
        }
    }

    public class OpenActionsEvent : AnalyticsEvent
    {
        public OpenActionsEvent(ModuleType module)
            : base(module, "actions_open")
        {
        }
    }

    public class OpenCommentsEvent : AnalyticsEvent
    {
        public OpenCommentsEvent(ModuleType module)
            : base(module, "comments_open")
        {
        }
    }

    public class OpenCategoriesEvent : AnalyticsEvent
    {
        public OpenCategoriesEvent(ModuleType module)
            : base(module, "categories_open")
        {
        }
    }

    public class OpenEditCategoriesEvent : AnalyticsEvent
    {
        public OpenEditCategoriesEvent(ModuleType module)
            : base(module, "edit_categories_open")
        {
        }
    }

    public class OpenSearchEvent : AnalyticsEvent
    {
        public OpenSearchEvent()
            : base("search_open")
        {
        }
    }

    public class OpenMailViewerEvent : AnalyticsEvent
    {
        public OpenMailViewerEvent()
            : base("mail_viewer_open")
        {
        }
    }

    public class OpenSettingsEvent : AnalyticsEvent
    {
        public OpenSettingsEvent()
            : base("settings_open")
        {
        }
    }

    public class OpenNotificationsEvent : AnalyticsEvent
    {
        public OpenNotificationsEvent(ModuleType module)
            : base(module, "notifications_open")
        {
        }
    }

    public class OpenAddContactEvent : AnalyticsEvent
    {
        public OpenAddContactEvent()
            : base(ModuleType.Contacts, "add_open")
        {
        }
    }

    public class OpenEditContactEvent : AnalyticsEvent
    {
        public OpenEditContactEvent()
            : base(ModuleType.Contacts, "edit_open")
        {
        }
    }

    public class OpenAddSubContactEvent : AnalyticsEvent
    {
        public OpenAddSubContactEvent()
            : base(ModuleType.Contacts, "add_subcontact_open")
        {
        }
    }

    public class OpenAddShortcodeEvent : AnalyticsEvent
    {
        public OpenAddShortcodeEvent()
            : base(ModuleType.Shortcodes, "add_open")
        {
        }
    }

    public class OpenEditShortcodeEvent : AnalyticsEvent
    {
        public OpenEditShortcodeEvent()
            : base(ModuleType.Shortcodes, "edit_open")
        {
        }
    }

    #endregion

    #region Swipe Related

    public class SwipeActionUsedEvent : AnalyticsEvent
    {
        public SwipeActionUsedEvent() : base("swipe_action_used")
        {
        }

    }

    public class SwipeActionChangedEvent : AnalyticsEvent
    {
        public SwipeActionChangedEvent() : base("swipe_action_changed")
        {
        }
    }

    #endregion
}