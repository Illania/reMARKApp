//
// Project: Mark5.Mobile.Common
// File: FolderContactLink.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using SQLite;

namespace Mark5.Mobile.Common.Model.Links
{

    [Table("FolderContactLink")]
    class FolderContactLink
    {

        [Column("FolderId"), Indexed]
        public int FolderId { get; set; } = -1;

        [Column("ContactId"), Indexed]
        public int ContactId { get; set; } = -1;
    }
}

