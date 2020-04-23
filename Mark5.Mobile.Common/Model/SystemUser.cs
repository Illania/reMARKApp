using System;

namespace Mark5.Mobile.Common.Model
{
    public class SystemUser
    {
        public int Id { get; set; } = -1;

        public Guid Guid { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string PatronymicName { get; set; }
        public string LastName { get; set; }
        public byte[] Avatar { get; set; }

        public string FullName
        {
            get
            {
                var fullName = string.Empty;
                if (!string.IsNullOrWhiteSpace(FirstName))
                    fullName += FirstName.Trim();
                if (!string.IsNullOrWhiteSpace(PatronymicName))
                    fullName += " " + PatronymicName.Trim();
                if (!string.IsNullOrWhiteSpace(LastName))
                    fullName += " " + LastName.Trim();
                return fullName.Trim();
            }
        }
    }
}