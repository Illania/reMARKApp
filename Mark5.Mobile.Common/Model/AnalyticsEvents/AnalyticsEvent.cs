using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Mark5.Mobile.Common.Model.AnalyticsEvents
{
    #region Abstract classes

    public abstract class AnalyticsEvent
    {
        public virtual string Name { get; protected set; }
        public List<AnalyticsParameter> Parameters { get; }

        protected string GetModuleString(ModuleType module)
        {
            return $"_{module.ToString().ToLower()}";
        }

        protected string GetQuantityString(int quantity)
        {
            return $"_{(quantity == 1 ? "single" : "multiple")}";
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

    public enum Quantity
    {
        Single,
        Multiple,
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
            Name = "copy_to_new_" + option.ToString(); //TODO check
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
    #region Documents

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

    #region Misc Events

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

    #endregion
}
