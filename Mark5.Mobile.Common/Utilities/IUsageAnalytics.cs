using System.Collections.Generic;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Common.Utilities
{
    public interface IUsageAnalytics
    {
        void LogEvent(AnalyticsEvent analyticsEvent);

        void SetUserProperty(UserProperty property, string value);
    }

    #region Enums

    public enum UserProperty
    {
        SSL,
        Hostname,
        Username
    }

    public enum ContactFastActionChoice
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
        Shortcodes,
        Phonebook,
    }

    public enum TemplateType
    {
        Local,
        Default,
        Another
    }

    public enum AddAttachmentType
    {
        Photo,
        Local
    }

    #endregion

    #region Abstract

    public abstract class AnalyticsEvent
    {
        public string EventName => eventName;

        protected readonly string eventName;

        protected AnalyticsEvent(string eventName)
        {
            this.eventName = eventName;
        }

        protected AnalyticsEvent(ModuleType module, string eventName)
        {
            this.eventName = module.ToString().ToLowerInvariant() + "_" + eventName;
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

            this.eventName = module.ToString().ToLowerInvariant() + "_" + eventName + "_" + quantityString;
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

    public class CopyToWorktrayEvent : AnalyticsEvent
    {
        public CopyToWorktrayEvent(ModuleType module, int quantity)
            : base(module, "copy_to_worktray", quantity)
        {
        }
    }

    public class CopyToUserWorktrayEvent : AnalyticsEvent
    {
        public CopyToUserWorktrayEvent(ModuleType module, int quantity)
            : base(module, "copy_to_user_worktray", quantity)
        {
        }
    }

    public class CopyToFolderEvent : AnalyticsEvent
    {
        public CopyToFolderEvent(ModuleType module, int quantity)
            : base(module, "copy_to_folder", quantity)
        {
        }
    }

    public class MoveToFolderEvent : AnalyticsEvent
    {
        public MoveToFolderEvent(ModuleType module, int quantity)
            : base(module, "move_to_folder", quantity)
        {
        }
    }

    public class DeleteEvent : AnalyticsEvent
    {
        public DeleteEvent(ModuleType module, int quantity)
            : base(module, "delete", quantity)
        {
        }
    }

    public class DeleteFromFolderEvent : AnalyticsEvent
    {
        public DeleteFromFolderEvent(ModuleType module, int quantity)
            : base(module, "delete_from_folder", quantity)
        {
        }
    }

    public class SetCategories : AnalyticsEvent
    {
        public SetCategories(ModuleType module, int quantity)
            : base(module, "set_categories", quantity)
        {
        }
    }

    public class AddCommentEvent : AnalyticsEvent
    {
        public AddCommentEvent(ModuleType module)
            : base(module, "add_comment")
        {
        }
    }

    public class EditCommentEvent : AnalyticsEvent
    {
        public EditCommentEvent(ModuleType module)
            : base(module, "edit_comment")
        {
        }
    }

    public class DeleteCommentEvent : AnalyticsEvent
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
        public OpenFolderEvent(ModuleType module)
            : base(module, "open_folder")
        {
        }
    }

    public class OpenLocalFolderEvent : AnalyticsEvent
    {
        public OpenLocalFolderEvent(ModuleType module)
            : base(module, "open_local_folder")
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
            : base("document_quick_switch")
        {
        }
    }

    public class DocumentOpenAttachmentEvent : AnalyticsEvent
    {
        public DocumentOpenAttachmentEvent()
            : base("document_open_attachment")
        {
        }
    }

    public class ReplyEvent : AnalyticsEvent
    {
        public ReplyEvent()
            : base(ModuleType.Documents, "reply")
        {
        }
    }

    public class ReplyAllEvent : AnalyticsEvent
    {
        public ReplyAllEvent()
            : base(ModuleType.Documents, "reply_all")
        {
        }
    }

    public class ForwardEvent : AnalyticsEvent
    {
        public ForwardEvent()
            : base(ModuleType.Documents, "forward")
        {
        }
    }

    public class CopyToNewEvent : AnalyticsEvent
    {
        public CopyToNewEvent(CopyToNewOption option)
            : base(ModuleType.Documents, "copy_to_new_" + option.ToString().ToLowerInvariant())
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
            : base("compose_add_attachment_" + type.ToString().ToLowerInvariant())
        {
        }
    }

    public class ComposeRemoveAttachmentEvent : AnalyticsEvent
    {
        public ComposeRemoveAttachmentEvent()
            : base("compose_remove_attachment")
        {
        }
    }

    public class ComposeOpenAttachment : AnalyticsEvent
    {
        public ComposeOpenAttachment()
            : base("compose_open_attachment")
        {
        }
    }

    public class ComposeShowPreviousEmailEvent : AnalyticsEvent
    {
        public ComposeShowPreviousEmailEvent()
            : base("compose_show_previous_email")
        {
        }
    }

    public class ComposeEditedPreviousEmailEvent : AnalyticsEvent
    {
        public ComposeEditedPreviousEmailEvent()
            : base("compose_edited_previous_email")
        {
        }
    }

    public class ComposeSaveDraftEvent : AnalyticsEvent
    {
        public ComposeSaveDraftEvent()
            : base("compose_save_draft")
        {
        }
    }

    public class ComposeAddTemplateEvent : AnalyticsEvent
    {
        public ComposeAddTemplateEvent(TemplateType? type)
            : base("compose_add_template_" + (type == null ? "_none" : type.ToString().ToLowerInvariant()))
        {
        }
    }

    public class ComposeContactPickerEvent : AnalyticsEvent
    {
        public ComposeContactPickerEvent(ContactPickerChoice type)
            : base("compose_contact_picker_" + type.ToString().ToLowerInvariant())
        {
        }
    }

    public class DocumentRecoveredEvent : AnalyticsEvent
    {
        public DocumentRecoveredEvent()
            : base("documents_recovered")
        {
        }
    }

    #endregion

    #region Contact events

    public class ContactFastActionEvent : AnalyticsEvent
    {
        public ContactFastActionEvent(ContactFastActionChoice type)
            : base("contact_fast_action_" + type.ToString().ToLowerInvariant())
        {
        }
    }

    public class ContactCallNumberEvent : AnalyticsEvent
    {
        public ContactCallNumberEvent()
            : base("contact_call_number")
        {
        }
    }

    public class ContactSendTextEvent : AnalyticsEvent
    {
        public ContactSendTextEvent()
            : base("contact_send_text")
        {
        }
    }

    public class ContactClickEmailEvent : AnalyticsEvent
    {
        public ContactClickEmailEvent()
            : base("contact_click_email")
        {
        }
    }

    public class ContactClickPhysicalAddressEvent : AnalyticsEvent
    {
        public ContactClickPhysicalAddressEvent()
            : base("contact_click_physical_address")
        {
        }
    }

    public class ContactNavigateSubContactEvent : AnalyticsEvent
    {
        public ContactNavigateSubContactEvent()
            : base("contact_navigate_subcontact")
        {
        }
    }

    #endregion

    #region Shortcode events

    public class ShortcodeClickEmailEvent : AnalyticsEvent
    {
        public ShortcodeClickEmailEvent()
            : base("shortcode_click_email")
        {
        }
    }

    public class ShortcodeComposeDocumentEvent : AnalyticsEvent
    {
        public ShortcodeComposeDocumentEvent()
            : base("shortcode_compose_document_event")
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

    public class SettingsLogOut : AnalyticsEvent
    {
        public SettingsLogOut()
            : base("settings_log_out")
        {
        }
    }

    #endregion

    #region View Opening events

    public class OpenDocumentEvent : AnalyticsEvent
    {
        public OpenDocumentEvent()
            : base("open_document")
        {
        }
    }

    public class OpenContactEvent : AnalyticsEvent
    {
        public OpenContactEvent()
            : base("open_contact")
        {
        }
    }

    public class OpenShortcodeEvent : AnalyticsEvent
    {
        public OpenShortcodeEvent()
            : base("open_shortcode")
        {
        }
    }

    public class OpenLinksEvent : AnalyticsEvent
    {
        public OpenLinksEvent(ModuleType module)
            : base(module, "open_links")
        {
        }
    }

    public class OpenActionsEvent : AnalyticsEvent
    {
        public OpenActionsEvent(ModuleType module)
            : base(module, "open_actions")
        {
        }
    }

    public class OpenCommentsEvent : AnalyticsEvent
    {
        public OpenCommentsEvent(ModuleType module)
            : base(module, "open_comments")
        {
        }
    }

    public class OpenCategoriesEvent : AnalyticsEvent
    {
        public OpenCategoriesEvent(ModuleType module)
            : base(module, "open_categories")
        {
        }
    }

    public class OpenSearchEvent : AnalyticsEvent
    {
        public OpenSearchEvent(ModuleType module)
            : base(module, "open_search")
        {
        }
    }

    public class OpenMailViewerEvent : AnalyticsEvent
    {
        public OpenMailViewerEvent()
            : base("open_mail_viewer")
        {
        }
    }

    public class OpenSettingsEvent : AnalyticsEvent
    {
        public OpenSettingsEvent()
            : base("open_settings")
        {
        }
    }

    public class OpenNotifications : AnalyticsEvent
    {
        public OpenNotifications(ModuleType module)
            : base(module, "open_notification")
        {
        }
    }

    public class AddContactEvent : AnalyticsEvent
    {
        public AddContactEvent()
            : base("add_contact")
        {
        }
    }

    public class EditContactEvent : AnalyticsEvent
    {
        public EditContactEvent()
            : base("edit_contact")
        {
        }
    }

    public class AddSubContactEvent : AnalyticsEvent
    {
        public AddSubContactEvent()
            : base("add_subcontact")
        {
        }
    }

    public class AddShortcodeEvent : AnalyticsEvent
    {
        public AddShortcodeEvent()
            : base("add_shortcode")
        {
        }
    }

    public class EditShortcodeEvent : AnalyticsEvent
    {
        public EditShortcodeEvent()
            : base("edit_shortcode")
        {
        }
    }

    public class ComposeNewDocumentEvent : AnalyticsEvent
    {
        public ComposeNewDocumentEvent()
            : base("compose_new_document")
        {
        }
    }

    public class ComposeEditDraftEvent : AnalyticsEvent
    {
        public ComposeEditDraftEvent()
            : base("compose_edit_draft")
        {
        }
    }

    #endregion

}