using System.Collections.Generic;

namespace Mark5.Mobile.Common.Model
{
    public class SystemUsersDepartments
    {
        List<SystemUser> users;

        public List<SystemUser> Users
        {
            get
            {
                if (users == null)
                    users = new List<SystemUser>();
                return users;
            }
            set => users = value;
        }

        List<SystemDepartment> departments;

        public List<SystemDepartment> Departments
        {
            get
            {
                if (departments == null)
                    departments = new List<SystemDepartment>();
                return departments;
            }
            set => departments = value;
        }
    }
}