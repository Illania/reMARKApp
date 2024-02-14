using System;
using System.Collections.Generic;
using reMark.Mobile.Common;
using reMark.Mobile.IOS.Ui.Common;

namespace reMark.Mobile.IOS.Utilities
{
    public class CategorySwipeAction
    {
        public enum SwipeAction
        {
            AddToFavorites,
            RemoveFromFavorites
        }

        public SwipeAction Action { get; set; }

        public CategorySwipeAction(SwipeAction action)
        {
            Action = action;
        }

        public CategorySwipeAction(string action)
        {
            Action = ParseStringToSwipeAction(action);
        }

        public static List<CategorySwipeAction> GetAllAvailableActions = new List<CategorySwipeAction>(new CategorySwipeAction[] {
            new CategorySwipeAction(SwipeAction.AddToFavorites),
            new CategorySwipeAction(SwipeAction.RemoveFromFavorites)
        });

        public string GetName()
        {
            return GetLocalizedName(Action);
        }

        public static string GetLocalizedName(SwipeAction action)
        {
            switch (action)
            {
                case SwipeAction.AddToFavorites:
                    return Localization.GetString("add_to_favorites");
                case SwipeAction.RemoveFromFavorites:
                    return Localization.GetString("remove_from_favorites");

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
