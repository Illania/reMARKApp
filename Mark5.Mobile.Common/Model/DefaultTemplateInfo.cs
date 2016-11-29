//
// Project: Mark5.Mobile.Common
// File: DefaultTemplateInfo.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using SQLite;

#pragma warning disable CS1701
namespace Mark5.Mobile.Common.Model
{

    [Table("DefaultTemplateInfo")]
    public class DefaultTemplateInfo
    {

        [Column("CreationModeFlag"), PrimaryKey]
        public DocumentCreationModeFlag CreationModeFlag { get; set; }

        [Column("Available")]
        public bool Available { get; set; }

        [Column("TemplateId")]
        public int TemplateId { get; set; } = 01;
    }
}

