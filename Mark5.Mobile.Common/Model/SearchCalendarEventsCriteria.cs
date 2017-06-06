//
// File: SearchCalendarEventsCriteria.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//

using System.Collections.Generic;

namespace Mark5.Mobile.Common.Model
{
    public class SearchCalendarEventsCriteria
    {
        public SearchCalendarEventsType Type { get; set; }

        public string SavedSearchFilterHash { get; set; }

        // Appointment, Task
        List<int> inCalendarOfUserIds;

        public List<int> InCalendarOfUserIds
        {
            get
            {
                if (inCalendarOfUserIds == null)
                {
                    inCalendarOfUserIds = new List<int>();
                }
                return inCalendarOfUserIds;
            }
            set { inCalendarOfUserIds = value; }
        }

        // Appointment, Task
        public Priority Priority { get; set; }

        // Appointment, Task
        public string Subject { get; set; }

        // Appointment, Task
        public string Description { get; set; }

        // Task
        List<int> inGroupCalendarOfUserIds;

        public List<int> InGroupCalendarOfUserIds
        {
            get
            {
                if (inGroupCalendarOfUserIds == null)
                {
                    inGroupCalendarOfUserIds = new List<int>();
                }
                return inGroupCalendarOfUserIds;
            }
            set { inGroupCalendarOfUserIds = value; }
        }

        // Task
        List<int> taskCreatedByUserIds;

        public List<int> TaskCreatedByUserIds
        {
            get
            {
                if (taskCreatedByUserIds == null)
                {
                    taskCreatedByUserIds = new List<int>();
                }
                return taskCreatedByUserIds;
            }
            set { taskCreatedByUserIds = value; }
        }

        // Task
        List<int> delegatedToUserIds;

        public List<int> DelegatedToUserIds
        {
            get
            {
                if (delegatedToUserIds == null)
                {
                    delegatedToUserIds = new List<int>();
                }
                return delegatedToUserIds;
            }
            set { delegatedToUserIds = value; }
        }

        // Task
        List<int> delegatedToDepartmentIds;

        public List<int> DelegatedToDepartmentIds
        {
            get
            {
                if (delegatedToDepartmentIds == null)
                {
                    delegatedToDepartmentIds = new List<int>();
                }
                return delegatedToDepartmentIds;
            }
            set { delegatedToDepartmentIds = value; }
        }

        // Appointment
        List<int> calendarCategoryIds;

        public List<int> CalendarCategoryIds
        {
            get
            {
                if (calendarCategoryIds == null)
                {
                    calendarCategoryIds = new List<int>();
                }
                return calendarCategoryIds;
            }
            set { calendarCategoryIds = value; }
        }

        // Appointment
        public string Location { get; set; }

        // Appointment
        List<int> participantUserIds;

        public List<int> ParticipantUserIds
        {
            get
            {
                if (participantUserIds == null)
                {
                    participantUserIds = new List<int>();
                }
                return participantUserIds;
            }
            set { participantUserIds = value; }
        }

        // Appointment, Task
        public DateRange DateRange { get; set; }

        // Appointment, Task
        public FiledInFolderType FiledInFolderType { get; set; }

        // Appointment, Task
        public FiledInFolderFolderType FiledInFolderFolderType { get; set; }

        // Appointment, Task
        List<int> filedInFolderIds;

        public List<int> FiledInFolderIds
        {
            get
            {
                if (filedInFolderIds == null)
                {
                    filedInFolderIds = new List<int>();
                }
                return filedInFolderIds;
            }
            set { filedInFolderIds = value; }
        }
    }
}