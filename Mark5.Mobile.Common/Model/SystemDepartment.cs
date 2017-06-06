using System;
using System.Collections.Generic;

namespace Mark5.Mobile.Common.Model
{
    public class SystemDepartment
    {
        public int Id { get; set; } = -1;

        public Guid Guid { get; set; }
        public string Name { get; set; }
        List<int> userIds;

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
    }
}