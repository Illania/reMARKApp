using System.Collections.Generic;

namespace Mark5.Mobile.Common.Model.AnalyticsEvents
{
    #region Abstract classes

    public abstract class AnalyticsEvent
    {
        public abstract string Name { get; }
        public List<AnalyticsParameter> Parameters { get; }
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

    public enum ActionOrigin
    {
        List,
        EntityView,
        Swype
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

    public enum SwitchDocumentType
    {
        Swype,
        Arrows,
    }

    #endregion

    #region General Events

    public class OpenNotificationListEvent : AnalyticsEvent
    {
        public override string Name => "open_notification_list";
    }

    public class OpenSubfolderEvent : AnalyticsEvent
    {
        public override string Name => "open_subfolder";

        readonly ModuleType module;
        readonly FolderType type;

        public OpenSubfolderEvent(ModuleType module, FolderType type)
        {
            this.module = module;
            this.type = type;
        }
    }

    public class AddToFavoriteEvent : AnalyticsEvent
    {
        public override string Name => "add_to_favorite";

        readonly ModuleType module;
        readonly int quantity;

        public AddToFavoriteEvent(ModuleType module, int quantity)
        {
            this.module = module;
            this.quantity = quantity;
        }
    }

    public class AddToSyncEvent : AnalyticsEvent
    {
        public override string Name => "add_to_sync";

        readonly ModuleType module;
        readonly int quantity;

        public AddToSyncEvent(ModuleType module, int quantity)
        {
            this.module = module;
            this.quantity = quantity;
        }
    }

    public class AddToNotifyEvent : AnalyticsEvent
    {
        public override string Name => "add_to_notify";

        readonly ModuleType module;
        readonly int quantity;

        public AddToNotifyEvent(ModuleType module, int quantity)
        {
            this.module = module;
            this.quantity = quantity;
        }
    }

    public class RemoveFromFavoriteEvent : AnalyticsEvent
    {
        public override string Name => "remove_from_favorite";

        readonly ModuleType module;
        readonly int quantity;

        public RemoveFromFavoriteEvent(ModuleType module, int quantity)
        {
            this.module = module;
            this.quantity = quantity;
        }
    }

    public class RemoveFromSyncEvent : AnalyticsEvent
    {
        public override string Name => "remove_from_sync";

        readonly ModuleType module;
        readonly int quantity;

        public RemoveFromSyncEvent(ModuleType module, int quantity)
        {
            this.module = module;
            this.quantity = quantity;
        }
    }

    public class RemoveFromNotifyEvent : AnalyticsEvent
    {
        public override string Name => "remove_from_notify";

        readonly ModuleType module;
        readonly int quantity;

        public RemoveFromNotifyEvent(ModuleType module, int quantity)
        {
            this.module = module;
            this.quantity = quantity;
        }
    }

    public class ExpandSubfolderEvent : AnalyticsEvent
    {
        public override string Name => "expand_subfolder";

        readonly ModuleType module;

        public ExpandSubfolderEvent(ModuleType module)
        {
            this.module = module;
        }
    }

    public class PullToRefreshEvent : AnalyticsEvent
    {
        public override string Name => "pull_to_refresh";

        readonly string viewName;

        public PullToRefreshEvent(string viewName)
        {
            this.viewName = viewName;
        }
    }

    public class FilteringEvent : AnalyticsEvent
    {
        public override string Name => "filtering";

        readonly string viewName;

        public FilteringEvent(string viewName)
        {
            this.viewName = viewName;
        }
    }

    //TODO How do we deal with the search fields?

    public class CopyToWorktrayEvent : AnalyticsEvent
    {
        public override string Name => "copy_to_worktray";

        readonly ModuleType module;
        readonly int quantity;
        readonly ActionOrigin origin;

        public CopyToWorktrayEvent(ModuleType module, int quantity, ActionOrigin origin)
        {
            this.module = module;
            this.quantity = quantity;
            this.origin = origin;
        }
    }

    public class CopyToFolderEvent : AnalyticsEvent
    {
        public override string Name => "copy_to_folder";

        readonly ModuleType module;
        readonly int quantity;
        readonly ActionOrigin origin;

        public CopyToFolderEvent(ModuleType module, int quantity, ActionOrigin origin)
        {
            this.module = module;
            this.quantity = quantity;
            this.origin = origin;
        }
    }

    public class DeleteEvent : AnalyticsEvent
    {
        public override string Name => "delete";

        readonly ModuleType module;
        readonly int quantity;
        readonly ActionOrigin origin;

        public DeleteEvent(ModuleType module, int quantity, ActionOrigin origin)
        {
            this.module = module;
            this.quantity = quantity;
            this.origin = origin;
        }
    }

    public class DeleteFromFolderEvent : AnalyticsEvent
    {
        public override string Name => "delete_from_folder";

        readonly ModuleType module;
        readonly int quantity;
        readonly ActionOrigin origin;

        public DeleteFromFolderEvent(ModuleType module, int quantity, ActionOrigin origin)
        {
            this.module = module;
            this.quantity = quantity;
            this.origin = origin;
        }
    }

    public class EditCategoriesEvent : AnalyticsEvent
    {
        public override string Name => "edit_categories";

        readonly ModuleType module;
        readonly int quantity;
        readonly ActionOrigin origin;

        public EditCategoriesEvent(ModuleType module, int quantity, ActionOrigin origin)
        {
            this.module = module;
            this.quantity = quantity;
            this.origin = origin;
        }
    }

    public class OpenLinksEvent : AnalyticsEvent
    {
        public override string Name => "open_links";

        readonly ModuleType module;

        public OpenLinksEvent(ModuleType module)
        {
            this.module = module;
        }
    }

    public class OpenActionsEvent : AnalyticsEvent
    {
        public override string Name => "open_actions";

        readonly ModuleType module;

        public OpenActionsEvent(ModuleType module)
        {
            this.module = module;
        }
    }

    public class OpenCommentsEvent : AnalyticsEvent
    {
        public override string Name => "open_comments";

        readonly ModuleType module;

        public OpenCommentsEvent(ModuleType module)
        {
            this.module = module;
        }
    }

    public class AddCommentEvent : AnalyticsEvent
    {
        public override string Name => "add_comment";

        readonly ModuleType module;

        public AddCommentEvent(ModuleType module)
        {
            this.module = module;
        }
    }

    public class RemoveCommentEvent : AnalyticsEvent
    {
        public override string Name => "remove_comment";

        readonly ModuleType module;

        public RemoveCommentEvent(ModuleType module)
        {
            this.module = module;
        }
    }

    public class EditCommentEvent : AnalyticsEvent
    {
        public override string Name => "edit_comment";

        readonly ModuleType module;

        public EditCommentEvent(ModuleType module)
        {
            this.module = module;
        }
    }

    #endregion

    #region Documents

    public class MarkDocumentAsReadEvent : AnalyticsEvent
    {
        public override string Name => "mark_as_read";

        readonly int quantity;
        readonly ActionOrigin origin;

        public MarkDocumentAsReadEvent(int quantity, ActionOrigin origin)
        {
            this.quantity = quantity;
            this.origin = origin;
        }
    }

    public class MarkDocumentAsUnreadEvent : AnalyticsEvent
    {
        public override string Name => "mark_as_unread";

        readonly int quantity;
        readonly ActionOrigin origin;

        public MarkDocumentAsUnreadEvent(int quantity, ActionOrigin origin)
        {
            this.quantity = quantity;
            this.origin = origin;
        }
    }

    public class SetDocumentPriorityEvent : AnalyticsEvent
    {
        public override string Name => "set_priority";

        readonly int quantity;
        readonly ActionOrigin origin;

        public SetDocumentPriorityEvent(int quantity, ActionOrigin origin)
        {
            this.quantity = quantity;
            this.origin = origin;
        }
    }

    public class AddAttachmentEvent : AnalyticsEvent
    {
        public override string Name => "add_attachment";
    }

    public class RemoveAttachmentEvent : AnalyticsEvent
    {
        public override string Name => "remove_attachment";
    }

    public class OpenAttachmentEvent : AnalyticsEvent
    {
        public override string Name => "open_attachment";
    }

    public class ContactPickerEvent : AnalyticsEvent
    {
        public override string Name => "contact_picker";

        readonly ContactPickerChoice choice;

        public ContactPickerEvent(ContactPickerChoice choice)
        {
            this.choice = choice;
        }
    }

    public class ReplyEvent : AnalyticsEvent
    {
        public override string Name => "reply";

        readonly ActionOrigin origin;

        public ReplyEvent(ActionOrigin origin)
        {
            this.origin = origin;
        }
    }

    public class ReplyAllEvent : AnalyticsEvent
    {
        public override string Name => "reply_all";

        readonly ActionOrigin origin;

        public ReplyAllEvent(ActionOrigin origin)
        {
            this.origin = origin;
        }
    }

    public class ForwardEvent : AnalyticsEvent
    {
        public override string Name => "forward";

        readonly ActionOrigin origin;

        public ForwardEvent(ActionOrigin origin)
        {
            this.origin = origin;
        }
    }

    public class CopyToNewEvent : AnalyticsEvent
    {
        public override string Name => "copy_to_new";

        readonly CopyToNewOption option;

        public CopyToNewEvent(CopyToNewOption option)
        {
            this.option = option;
        }
    }

    public class TemplateAddedEvent : AnalyticsEvent
    {
        public override string Name => "template_added";

        readonly TemplateType type;

        public TemplateAddedEvent(TemplateType type)
        {
            this.type = type;
        }
    }

    public class SaveDocumentAsDraft : AnalyticsEvent
    {
        public override string Name => "save_document_draft";
    }

    public class OpenMailViewer : AnalyticsEvent
    {
        public override string Name => "open_mail_viewer";
    }

    public class SwitchDocument : AnalyticsEvent
    {
        public override string Name => "switch_document";

        readonly SwitchDocumentType type;

        public SwitchDocument(SwitchDocumentType type)
        {
            this.type = type;
        }
    }

    #endregion

    #region Contacts

    #endregion

    #region Shortcodes

    #endregion
}
