//
// File: FolderCalendarTaskLink.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//

using SQLite;

namespace Mark5.Mobile.Common.Model.Links
{
    [Table("FolderCalendarTaskLink")]
    class FolderCalendarTaskLink
    {
        [Column("FolderId"), Indexed]
        public int FolderId { get; set; } = -1;

        [Column("CalendarTaskId"), Indexed]
        public int CalendarTaskId { get; set; } = -1;
    }
}