//
// Project: Mark5.Mobile.Common
// File: Notification.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using SQLite;

namespace Mark5.Mobile.Common.Model
{

    [Table("Notification")]
    public class Notification
    {

        [Column("Id"), PrimaryKey, AutoIncrement]
        public int Id { get; set; } = -1;

        [Column("Guid")]
        public Guid Guid { get; set; }

        [Column("Title")]
        public string Title { get; set; }

        [Column("Message")]
        public string Message { get; set; }

        [Column("Type")]
        public EventType Type { get; set; }

        [Column("DateTime")]
        public DateTime DateTime { get; set; }

        [Column("ObjectType")]
        public ObjectType ObjectType { get; set; }

        [Column("ObjectId")]
        public int ObjectId { get; set; } = -1;

        [Column("FolderId")]
        public int FolderId { get; set; } = -1;

        [Column("RemindOn")]
        public DateTime RemindOn { get; set; }

        [Column("IsSilent")]
        public bool IsSilent { get; set; }
    }
}

