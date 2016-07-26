//
// Project: Mark5.Mobile.Common
// File: Template.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using SQLite;

namespace Mark5.Mobile.Common.Model
{

    [Table("Template")]
    public class Template
    {

        [Column("Id"), PrimaryKey]
        public int Id { get; set; } = -1;

        [Column("Guid")]
        public Guid Guid { get; set; }

        [Column("Subject")]
        public string Subject { get; set; }

        [Column("LineGuid")]
        public Guid LineGuid { get; set; }

        [Column("ContentType")]
        public ContentType ContentType { get; set; }

        [Column("Content")]
        public string Content { get; set; }

        public override string ToString()
        {
            return $"[Template: Id={Id}]";
        }
    }
}

