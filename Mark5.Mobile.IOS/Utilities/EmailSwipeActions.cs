using System;
using System.Collections.Generic;
using Foundation;
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
            Delete
        }

        SwipeAction _action;
        public SwipeAction Action
        {
            set {
                this.SelectedAction = value.ToString();
                _action = value;
            }

            get {
                return _action;
            }
        }

        string SelectedAction {
            get; set;
        }

        public EmailSwipeAction(SwipeAction action)
        {
            Action = action;
        }

        public EmailSwipeAction(string action)
        {
            Action = ParseStringToSwipeAction(action);
        }

        public static List<EmailSwipeAction> GetAllAvailableActions = new List<EmailSwipeAction>(new EmailSwipeAction[] {
            new EmailSwipeAction(SwipeAction.MarkAsRead),
            new EmailSwipeAction(SwipeAction.CopyToWorkTray),
            new EmailSwipeAction(SwipeAction.CopyToFolder),
            new EmailSwipeAction(SwipeAction.Categories),
            new EmailSwipeAction(SwipeAction.MoveToFolder),
            new EmailSwipeAction(SwipeAction.SetPriority),
            new EmailSwipeAction(SwipeAction.RemoveFromFolder),
            new EmailSwipeAction(SwipeAction.Delete)
        });

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

                default:
                    return "FORGOT A CASE?";
            }

        }

        public static EmailSwipeAction.SwipeAction ParseStringToSwipeAction(string value)
        {
            return (EmailSwipeAction.SwipeAction)Enum.Parse(typeof(EmailSwipeAction.SwipeAction), value, true);
        }
    }
}
