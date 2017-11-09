using System.Collections.Generic;

namespace Mark5.Mobile.Common.Model.AnalyticsEvents
{
    #region Abstract classes

    public abstract class AnalyticsEvent
    {
        public virtual string Name { get; protected set; }
        public List<AnalyticsParameter> Parameters { get; }

        protected string GetModuleString(ModuleType module)
        {
            return $"_{module.ToString().ToLowerInvariant()}";
        }

        protected string GetQuantityString(int quantity)
        {
            return $"_{(quantity == 1 ? "single" : "multiple")}";
        }

        protected string GetOptionString(CopyToNewOption option)
        {
            switch (option)
            {
                case CopyToNewOption.KeepOnlyAddresses:
                    return "_keep_only_addresses";
                case CopyToNewOption.KeepOnlyAttachments:
                    return "_keep_only_attachments";
                case CopyToNewOption.KeepTextAndAttachments:
                    return "_keep_text_and_attachments";
                default:
                    return "_none";
            }
        }

        protected string GetObjectTypeString(ObjectType type)
        {
            return $"_{type.ToString().ToLowerInvariant()}";
        }
    }

    public abstract class AnalyticsParameter
    {
        public string Name { get; protected set; }
    }

    public abstract class StringAnalyticsParameter : AnalyticsParameter
    {
        public string Value { get; private set; }

        protected StringAnalyticsParameter(string name, string stringValue)
        {
            Name = name;
            Value = stringValue;
        }
    }

    public abstract class NumberAnalyticsParameter : AnalyticsParameter
    {
        public int Value { get; private set; }

        protected NumberAnalyticsParameter(string name, int numberValue)
        {
            Name = name;
            Value = numberValue;
        }
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

    #region Actions Events

    public class SetFavoriteEvent : AnalyticsEvent
    {
        public SetFavoriteEvent(ModuleType module, int quantity)
        {
            Name = "set_favorite" + GetModuleString(module) + GetQuantityString(quantity);
        }
    }

    public class SetSyncEvent : AnalyticsEvent
    {
        public SetSyncEvent(ModuleType module, int quantity)
        {
            Name = "set_sync" + GetModuleString(module) + GetQuantityString(quantity);
        }
    }

    public class SetNotifyEvent : AnalyticsEvent
    {
        public SetNotifyEvent(ModuleType module, int quantity)
        {
            Name = "set_notify" + GetModuleString(module) + GetQuantityString(quantity);
        }
    }

    public class SetReadStatusEvent : AnalyticsEvent
    {
        public SetReadStatusEvent(int quantity)
        {
            Name = "set_read_status" + GetQuantityString(quantity);
        }
    }

    public class CopyToWorktrayEvent : AnalyticsEvent
    {
        public CopyToWorktrayEvent(ModuleType module, int quantity)
        {
            Name = "copy_to_worktray" + GetModuleString(module) + GetQuantityString(quantity);
        }
    }

    public class CopyToFolderEvent : AnalyticsEvent
    {
        public CopyToFolderEvent(ModuleType module, int quantity)
        {
            Name = "copy_to_folder" + GetModuleString(module) + GetQuantityString(quantity);
        }
    }

    public class DeleteEvent : AnalyticsEvent
    {
        public DeleteEvent(ModuleType module, int quantity)
        {
            Name = "delete" + GetModuleString(module) + GetQuantityString(quantity);
        }
    }

    public class DeleteFromFolderEvent : AnalyticsEvent
    {
        public DeleteFromFolderEvent(ModuleType module, int quantity)
        {
            Name = "delete_from_folder" + GetModuleString(module) + GetQuantityString(quantity);
        }
    }

    public class SetPriorityEvent : AnalyticsEvent
    {
        public SetPriorityEvent(int quantity)
        {
            Name = "set_priority" + GetQuantityString(quantity);
        }
    }

    public class SetCategories : AnalyticsEvent
    {
        public SetCategories(ModuleType module)
        {
            Name = "set_priority" + GetModuleString(module);
        }
    }

    public class ReplyEvent : AnalyticsEvent
    {
        public override string Name => "reply";
    }

    public class ReplyAllEvent : AnalyticsEvent
    {
        public override string Name => "reply_all";
    }

    public class ForwardEvent : AnalyticsEvent
    {
        public override string Name => "forward";
    }

    public class CopyToNewEvent : AnalyticsEvent
    {
        public CopyToNewEvent(CopyToNewOption option)
        {
            Name = "copy_to_new_" + GetOptionString(option);
        }
    }

    public class AddCommentEvent : AnalyticsEvent
    {
        public AddCommentEvent(ModuleType module)
        {
            Name = "add_comment" + GetModuleString(module);
        }
    }

    public class RemoveCommentEvent : AnalyticsEvent
    {
        public RemoveCommentEvent(ModuleType module)
        {
            Name = "remove_comment" + GetModuleString(module);
        }
    }

    public class EditCommentEvent : AnalyticsEvent
    {
        public EditCommentEvent(ModuleType module)
        {
            Name = "edit_comment" + GetModuleString(module);
        }
    }

    #endregion

    #region View Opening events

    public class OpenDocumentEvent : AnalyticsEvent
    {
        public OpenDocumentEvent(bool isExternal)
        {
            Name = "open_document" + $"{(isExternal ? "_is_external" : "")}";
        }
    }

    public class OpenContactEvent : AnalyticsEvent
    {
        public override string Name => "open_contact";
    }

    public class OpenShortcodeEvent : AnalyticsEvent
    {
        public override string Name => "open_shortcode";
    }

    public class OpenLinksEvent : AnalyticsEvent
    {
        public OpenLinksEvent(ModuleType module)
        {
            Name = "open_links" + GetModuleString(module);
        }
    }

    public class OpenActionsEvent : AnalyticsEvent
    {
        public OpenActionsEvent(ModuleType module)
        {
            Name = "open_actions" + GetModuleString(module);
        }
    }

    public class OpenCommentsEvent : AnalyticsEvent
    {
        public OpenCommentsEvent(ModuleType module)
        {
            Name = "open_comments" + GetModuleString(module);
        }
    }

    public class OpenCategoriesEvent : AnalyticsEvent
    {
        public OpenCategoriesEvent(ModuleType module)
        {
            Name = "open_categories" + GetModuleString(module);
        }
    }

    public class OpenSearchEvent : AnalyticsEvent
    {
        public OpenSearchEvent(ModuleType module)
        {
            Name = "open_search" + GetModuleString(module);
        }
    }

    public class OpenMailViewerEvent : AnalyticsEvent
    {
        public override string Name => "open_mail_viewer";
    }

    public class OpenSettingsEvent : AnalyticsEvent
    {
        public override string Name => "open_settings";
    }

    public class OpenNotificationListEvent : AnalyticsEvent
    {
        public override string Name => "open_notification_list";
    }

    public class AddContactEvent : AnalyticsEvent
    {
        public override string Name => "add_contact";
    }

    public class EditContactEvent : AnalyticsEvent
    {
        public override string Name => "edit_contact";
    }

    public class AddSubContactEvent : AnalyticsEvent
    {
        public override string Name => "add_subcontact";
    }

    public class AddShortcodeEvent : AnalyticsEvent
    {
        public override string Name => "add_shortcode";
    }

    public class EditShortcodeEvent : AnalyticsEvent
    {
        public override string Name => "edit_shortcode";
    }

    public class ComposeNewDocumentEvent : AnalyticsEvent
    {
        public override string Name => "compose_new_document";
    }

    public class ComposeEditDraft : AnalyticsEvent
    {
        public override string Name => "compose_edit_draft";
    }

    #endregion

    #region Documents

    public class ComposeAddAttachment : AnalyticsEvent
    {
        public ComposeAddAttachment(AddAttachmentType type)
        {
            Name = "compose_add_attachment_" + type.ToString().ToLowerInvariant();
        }
    }

    public class ComposeRemoveAttachmentEvent : AnalyticsEvent
    {
        public override string Name => "compose_remove_attachment";
    }

    public class ComposeShowPreviousEmailEvent : AnalyticsEvent
    {
        public override string Name => "compose_show_previous_email";
    }

    public class ComposeEditedPreviousEmailEvent : AnalyticsEvent
    {
        public override string Name => "compose_edited_previous_email";
    }

    public class ComposeEmailRecoveredEvent : AnalyticsEvent
    {
        public override string Name => "compose_email_recovered_event";
    }

    public class ComposeSaveDraftEvent : AnalyticsEvent
    {
        public override string Name => "compose_save_draft";
    }

    public class ComposeAddTemplateEvent : AnalyticsEvent
    {
        public ComposeAddTemplateEvent(TemplateType type)
        {
            Name = "compose_add_template_" + type.ToString().ToLowerInvariant();
        }
    }

    public class ComposeContactPickerEvent : AnalyticsEvent
    {
        public ComposeContactPickerEvent(ContactPickerChoice type)
        {
            Name = "compose_contact_picker_" + type.ToString().ToLowerInvariant();
        }
    }

    public class DocumentQuickSwitchEvent : AnalyticsEvent
    {
        public override string Name => "document_quick_switch";
    }


    public class DocumentOpenAttachment : AnalyticsEvent
    {
        public override string Name => "document_open_attachment";
    }

    public class DocumentShowDetail : AnalyticsEvent
    {
        public override string Name => "document_show_detail";
    }

    public class GetMoreDocumentsEvent : AnalyticsEvent
    {
        public override string Name => "filtering";
    }

    #endregion

    #region Contacts

    public class ContactFastActionEvent : AnalyticsEvent
    {
        public ContactFastActionEvent(ContactFastActionChoice type)
        {
            Name = "contact_fast_action_" + type.ToString().ToLowerInvariant();
        }
    }

    public class ContactCallNumberEvent : AnalyticsEvent
    {
        public override string Name => "contact_call_number";
    }

    public class ContactClickEmailEvent : AnalyticsEvent
    {
        public override string Name => "contact_click_email";
    }

    public class ContactClickPhysicalAddressEvent : AnalyticsEvent
    {
        public override string Name => "contact_click_physical_address";
    }

    public class ContactNavigateSubContactEvent : AnalyticsEvent
    {
        public override string Name => "contact_navigate_subcontact";
    }

    #endregion

    #region Shortcodes

    public class ShortcodeClickEmailEvent : AnalyticsEvent
    {
        public override string Name => "shortcode_click_email";
    }

    public class ShortcodeComposeDocumentEvent : AnalyticsEvent
    {
        public override string Name => "ShortcodeComposeDocumentEvent";
    }

    #endregion

    #region Settings

    public class SettingsUpdateSystemConfigurationEvent : AnalyticsEvent
    {
        public override string Name => "settings_update_system_configuration";
    }

    public class SettingsCacheCleanUpEvent : AnalyticsEvent
    {
        public override string Name => "settings_cache_clean_up";
    }

    public class SettingsLogOut : AnalyticsEvent
    {
        public override string Name => "settings_log_out";
    }

    #endregion

    #region Misc Events

    public class NotificationClickedEvent : AnalyticsEvent
    {
        public NotificationClickedEvent(ObjectType objectType)
        {
            Name = "notification_clicked" + GetObjectTypeString(objectType);
        }
    }
    public class NotificationMarkAllAsReadEvent : AnalyticsEvent
    {
        public override string Name => "notification_mark_all_as_read";
    }

    public class OpenLocalFolderEvent : AnalyticsEvent
    {
        public override string Name => "open_local_folder";
    }

    public class OpenFolderEvent : AnalyticsEvent
    {
        public OpenFolderEvent(ModuleType module, bool favorite)
        {
            Name = "open_folder" + GetModuleString(module);
            if (favorite)
            {
                Name += "_favorite";
            }
        }
    }

    public class ExpandFolderEvent : AnalyticsEvent
    {
        public ExpandFolderEvent(ModuleType module)
        {
            Name = "expand_folder" + GetModuleString(module);
        }
    }

    public class PullToRefreshEvent : AnalyticsEvent
    {
        public PullToRefreshEvent(bool fromFolder = false, ModuleType module = ModuleType.None)
        {
            Name = "pull_to_refresh";
            if (fromFolder == true)
            {
                Name += "_folder";
            }
            else
            {
                Name += GetModuleString(module);
            }
        }
    }

    public class FilterEvent : AnalyticsEvent
    {
        public FilterEvent(bool fromFolder = false, ModuleType module = ModuleType.None)
        {
            Name = "filter";
            if (fromFolder == true)
            {
                Name += "_folder";
            }
            else
            {
                Name += GetModuleString(module);
            }
        }
    }

    #endregion
}
