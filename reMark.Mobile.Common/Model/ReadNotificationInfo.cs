using System;
using SQLite;

namespace reMark.Mobile.Common.Model
{
    [Table("ReadNotificationInfo")]
    public class ReadNotificationInfo
    {
        [Column("NotificationGuid")]
        [NotNull]
        [Unique]
        public Guid NotificationGuid { get; set; }
    }
}