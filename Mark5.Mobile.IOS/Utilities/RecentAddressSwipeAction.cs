using System;
using System.Collections.Generic;
using Mark5.Mobile.Common;
using Mark5.Mobile.IOS.Ui.Common;

namespace Mark5.Mobile.IOS.Utilities
{
    public class RecentAddressSwipeAction
    {
        public enum SwipeAction
        {
            Delete
        }

        public SwipeAction Action { get; set; }

        public RecentAddressSwipeAction(SwipeAction action)
        {
            Action = action;
        }

        public RecentAddressSwipeAction(string action)
        {
            Action = ParseStringToSwipeAction(action);
        }

        public static List<RecentAddressSwipeAction> GetAllAvailableActions = new List<RecentAddressSwipeAction>(new RecentAddressSwipeAction[] {
            new RecentAddressSwipeAction(SwipeAction.Delete)
        });

        public string GetName()
        {
            return GetLocalizedName(Action);
        }

        public static string GetLocalizedName(SwipeAction action)
        {
            switch (action)
            {
                case SwipeAction.Delete:
                    return Localization.GetString("delete");

                default:
                    CommonConfig.Logger.Error($"Missing implementation for case : {action.ToString()}");
                    return "";
            }

        }

        public static SwipeAction ParseStringToSwipeAction(string value)
        {
            return (SwipeAction)Enum.Parse(typeof(SwipeAction), value, true);
        }
    }
}
