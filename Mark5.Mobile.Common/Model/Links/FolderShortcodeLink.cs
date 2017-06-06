//
// File: FolderShortcodeLink.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//

using SQLite;

namespace Mark5.Mobile.Common.Model.Links
{
    [Table("FolderShortcodeLink")]
    class FolderShortcodeLink
    {
        [Column("FolderId"), Indexed]
        public int FolderId { get; set; } = -1;

        [Column("ShortcodeId"), Indexed]
        public int ShortcodeId { get; set; } = -1;
    }
}