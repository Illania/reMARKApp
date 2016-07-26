//
// Project: Mark5.Mobile.Common
// File: TemplatePreview.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using SQLite;

namespace Mark5.Mobile.Common.Model
{

    [Table("TemplatePreview")]
    public class TemplatePreview
    {

        [Column("Id"), PrimaryKey]
        public int Id { get; set; } = -1;

        [Column("Guid"), NotNull]
        public Guid Guid { get; set; }

        [Column("Name")]
        public string Name { get; set; }

        [Column("Private")]
        public bool Private { get; set; }

        [Column("CreationMode")]
        public DocumentCreationModeFlag CreationMode { get; set; }

        public override string ToString()
        {
            return $"[TemplatePreview: Id={Id}, Name={Name}, Private={Private}]";
        }
    }
}

