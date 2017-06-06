using System;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Utilities
{
    public static class UI
    {
        public static int PriorityResourceId(Priority priority)
        {
            switch (priority)
            {
                case Priority.Low:
                    return Resource.String.priority_low;
                case Priority.Normal:
                    return Resource.String.priority_normal;
                case Priority.Urgent:
                    return Resource.String.priority_urgent;
                default:
                    throw new ArgumentException("The input priority should not be shown to the user");
            }
        }
    }
}