using System;
using System.Collections.Generic;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.IOS.Ui.Common;

namespace Mark5.Mobile.IOS.Utilities
{
    public class EmailSwipeAction
    {
        public enum SwipeAction
        {
            More,
            MarkAsRead,
            CopyToWorkTray,
            CopyToFolder,
            Categories,
            MoveToFolder,
            SetPriority,
            RemoveFromFolder,
            Delete,
            SetPresetCategory,
            AddBookmark
        }

        public SwipeAction Action { get; set; }

        public EmailSwipeAction(SwipeAction action)
        {
            Action = action;
        }

        public EmailSwipeAction(string action)
        {
            Action = ParseStringToSwipeAction(action);
        }

        public static List<EmailSwipeAction> GetAllAvailableActions()
        {
            List<EmailSwipeAction> list = new List<EmailSwipeAction>();
            list.Add(new EmailSwipeAction(SwipeAction.MarkAsRead));
            list.Add(new EmailSwipeAction(SwipeAction.CopyToWorkTray));
            list.Add(new EmailSwipeAction(SwipeAction.CopyToFolder));
            list.Add(new EmailSwipeAction(SwipeAction.Categories));
            if (PlatformConfig.Preferences.EnableMoveToFolder)
                list.Add(new EmailSwipeAction(SwipeAction.MoveToFolder));
            list.Add(new EmailSwipeAction(SwipeAction.SetPriority));
            list.Add(new EmailSwipeAction(SwipeAction.RemoveFromFolder));
            list.Add(new EmailSwipeAction(SwipeAction.Delete));
            list.Add(new EmailSwipeAction(SwipeAction.SetPresetCategory));
            list.Add(new EmailSwipeAction(SwipeAction.AddBookmark));
            return list;
        }

        public string GetName() {
            return GetLocalizedName(Action);
        }

        public static string GetLocalizedName(EmailSwipeAction.SwipeAction action)
        {
            switch (action)
            {
                case EmailSwipeAction.SwipeAction.MarkAsRead:
                    return Localization.GetString("mark_as_read_unread");

                case EmailSwipeAction.SwipeAction.CopyToWorkTray:
                    return Localization.GetString("copy_to_worktray_ml");
                case EmailSwipeAction.SwipeAction.Delete:
                    return Localization.GetString("delete");

                case EmailSwipeAction.SwipeAction.Categories:
                    return Localization.GetString("categories");

                case EmailSwipeAction.SwipeAction.CopyToFolder:
                    return Localization.GetString("copy_to_folder");

                case EmailSwipeAction.SwipeAction.MoveToFolder:
                    return Localization.GetString("move_to_folder");

                case EmailSwipeAction.SwipeAction.SetPriority:
                    return Localization.GetString("set_priority");

                case EmailSwipeAction.SwipeAction.RemoveFromFolder:
                    return Localization.GetString("delete_from_folder");

                case EmailSwipeAction.SwipeAction.SetPresetCategory:
                    return Localization.GetString("set_preset_category");

                case EmailSwipeAction.SwipeAction.AddBookmark:
                    return Localization.GetString("add_bookmark");

                default:
                    CommonConfig.Logger.Error($"Missing implementation for case : {action.ToString()}");
                    return "";
            }

        }

        public static EmailSwipeAction.SwipeAction ParseStringToSwipeAction(string value)
        {
            return (EmailSwipeAction.SwipeAction)Enum.Parse(typeof(EmailSwipeAction.SwipeAction), value, true);
        }
    }
}
