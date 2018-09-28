using System;
using System.Collections.Generic;
using Mark5.Mobile.Common.Utilities;
using SQLite;

namespace Mark5.Mobile.Common.Model
{
    public class CalendarTask : BusinessEntity
    {
        [Ignore]
        public override ObjectType ObjectType => ObjectType.CalendarTask;

        [Ignore]
        public override ModuleType ModuleType => ModuleType.Calendar;

        [Column("Subject")]
        public string Subject { get; set; }

        [Column("StartDateTimestamp")]
        public long StartDateTimestamp { get; set; } = -1;

        [Column("EndDateTimestamp")]
        public long EndDateTimestamp { get; set; } = -1;

        [Column("Private")]
        public bool Private { get; set; }

        [Column("Status")]
        public TaskStatus Status { get; set; }

        [Column("CreatorId")]
        public int CreatorId { get; set; } = -1;

        [Column("Creator")]
        public string Creator { get; set; }

        [Column("Priority")]
        public Priority Priority { get; set; }

        [Column("Type")]
        public CalendarOccurenceType Type { get; set; }

        [Column("ReminderDateTimestamp")]
        public long ReminderDateTimestamp { get; set; } = -1;

        [Column("SnoozeDelay")]
        public long SnoozeDelay { get; set; } = -1;

        [Column("PercentComplete")]
        public int PercentComplete { get; set; }

        [Column("LinkedObjectType")]
        public ObjectType LinkedObjectType { get; set; }

        [Column("LinkedObjectId")]
        public int LinkedObjectId { get; set; } = -1;

        [Column("DelegatorId")]
        public int DelegatorId { get; set; } = -1;

        [Column("Delegator")]
        public string Delegator { get; set; }

        [Column("CalendarId")]
        public int CalendarId { get; set; } = -1;

        List<int> userIds;

        [Ignore]
        public List<int> UserIds
        {
            get
            {
                if (userIds == null)
                    userIds = new List<int>();
                return userIds;
            }
            set => userIds = value;
        }

        Dictionary<int, string> users;

        [Ignore]
        public Dictionary<int, string> Users
        {
            get
            {
                if (users == null)
                    users = new Dictionary<int, string>();
                return users;
            }
            set => users = value;
        }

        List<int> departmentIds;

        [Ignore]
        public List<int> DepartmentIds
        {
            get
            {
                if (departmentIds == null)
                    departmentIds = new List<int>();
                return departmentIds;
            }
            set => departmentIds = value;
        }

        Dictionary<int, string> departments;

        [Ignore]
        public Dictionary<int, string> Departments
        {
            get
            {
                if (departments == null)
                    departments = new Dictionary<int, string>();
                return departments;
            }
            set => departments = value;
        }

        [Column("DelegationStatus")]
        public DelegationStatus DelegationStatus { get; set; }

        #region Serialization

        [Column("UserIdsString")]
        public string UserIdsString { get => Serializer.Serialize(UserIds); set => UserIds = Serializer.Deserialize<List<int>>(value); }

        [Column("UsersString")]
        public string UsersString { get => Serializer.Serialize(Users); set => Users = Serializer.Deserialize<Dictionary<int, string>>(value); }

        [Column("DepartmentIdsString")]
        public string DepartmentIdsString { get => Serializer.Serialize(DepartmentIds); set => DepartmentIds = Serializer.Deserialize<List<int>>(value); }

        [Column("DepartmentsString")]
        public string DepartmentsString { get => Serializer.Serialize(Departments); set => Departments = Serializer.Deserialize<Dictionary<int, string>>(value); }

        #endregion

        public override string ToString()
        {
            return $"[CalendarTask: Subject={Subject}, StartDateTimestamp={StartDateTimestamp}, EndDateTimestamp={EndDateTimestamp}, Status={Status}]";
        }
    }
}