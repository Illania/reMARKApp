//
// Project: Mark5.Mobile.Common
// File: CalendarTask.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Collections.Generic;

namespace Mark5.Mobile.Common.Model
{

    public class CalendarTask : BusinessEntity
    {

        public override ObjectType ObjectType
        {
            get
            {
                return ObjectType.CalendarTask;
            }
        }

        public override ModuleType ModuleType
        {
            get
            {
                return ModuleType.Calendar;
            }
        }

        public string Subject { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public bool Private { get; set; }

        public TaskStatus Status { get; set; }

        public int CreatorId { get; set; } = -1;

        public string Creator { get; set; }

        public Priority Priority { get; set; }

        public CalendarOccurenceType Type { get; set; }

        public DateTime ReminderDate { get; set; }

        public long SnoozeDelay { get; set; } = -1;

        public int PercentComplete { get; set; }

        public ObjectType LinkedObjectType { get; set; }

        public int LinkedObjectId { get; set; } = -1;

        public int DelegatorId { get; set; } = -1;

        public string Delegator { get; set; }

        List<int> userIds;

        public List<int> UserIds
        {
            get
            {
                if (userIds == null)
                {
                    userIds = new List<int>();
                }

                return userIds;
            }
            set
            {
                userIds = value;
            }
        }

        Dictionary<int, string> users;

        public Dictionary<int, string> Users
        {
            get
            {
                if (users == null)
                {
                    users = new Dictionary<int, string>();
                }

                return users;
            }
            set
            {
                users = value;
            }
        }

        List<int> departmentIds;

        public List<int> DepartmentIds
        {
            get
            {
                if (departmentIds == null)
                {
                    departmentIds = new List<int>();
                }

                return departmentIds;
            }
            set
            {
                departmentIds = value;
            }
        }

        Dictionary<int, string> departments;

        public Dictionary<int, string> Departments
        {
            get
            {
                if (departments == null)
                {
                    departments = new Dictionary<int, string>();
                }

                return departments;
            }
            set
            {
                departments = value;
            }
        }

        public DelegationStatus DelegationStatus { get; set; }
    }
}

