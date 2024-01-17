using System;
using reMark.Mobile.Common.Model;

namespace reMark.Mobile.Droid.Utilities
{
    public static class UIResource
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

        public static int ContactTypeResourceId(ContactType type)
        {
            switch (type)
            {
                case ContactType.Person:
                    return Resource.String.person;
                case ContactType.Company:
                    return Resource.String.company;
                case ContactType.Department:
                    return Resource.String.department;
                default:
                    throw new ArgumentException("Input type not valid");
            }
        }
    }
}