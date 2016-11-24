//
// Project: Mark5.Mobile.Common
// File: Notification.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using SQLite;

#pragma warning disable CS1701
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

        [Column("DateTimeTimestamp")]
        public long DateTimeTimestamp { get; set; } = -1;

        [Column("ObjectType")]
        public ObjectType ObjectType { get; set; }

        [Column("ObjectId")]
        public int ObjectId { get; set; } = -1;

        [Column("FolderId")]
        public int FolderId { get; set; } = -1;

        [Column("RemindOnTimestamp")]
        public long RemindOnTimestamp { get; set; } = -1;

        [Column("IsSilent")]
        public bool IsSilent { get; set; }

        [Ignore]
        public bool IsRead { get; set; }

        public override string ToString()
        {
            return $"[Notification: Id={Id}, Guid={Guid}, Title={Title}, Message={Message}, Type={Type}, DateTimeTimestamp={DateTimeTimestamp}, ObjectType={ObjectType}, ObjectId={ObjectId}, FolderId={FolderId}, RemindOnTimestamp={RemindOnTimestamp}, IsSilent={IsSilent}, IsRead={IsRead}]";
        }
    }
}

