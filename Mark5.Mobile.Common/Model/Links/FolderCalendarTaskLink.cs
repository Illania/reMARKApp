//
// Project: Mark5.Mobile.Common
// File: FolderCalendarTaskLink.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using SQLite;

#pragma warning disable CS1701
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

