//
// Project: Mark5.Mobile.Common
// File: ReadNotificationInfo.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using SQLite;

namespace Mark5.Mobile.Common.Model
{

    [Table("ReadNotificationInfo")]
    public class ReadNotificationInfo
    {

        [Column("Id"), PrimaryKey]
        public int Id { get; set; } = -1;

        [Column("NotificationGuid")]
        public Guid NotificationGuid { get; set; }
    }
}
