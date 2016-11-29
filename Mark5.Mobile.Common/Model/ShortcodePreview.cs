//
// Project: Mark5.Mobile.Common
// File: ShortcodePreview.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using SQLite;

#pragma warning disable CS1701
namespace Mark5.Mobile.Common.Model
{

    [Table("ShortcodePreview")]
    public class ShortcodePreview : BusinessEntityPreview
    {

        [Ignore]
        public override ObjectType ObjectType
        {
            get
            {
                return ObjectType.Shortcode;
            }
        }

        [Ignore]
        public override ModuleType ModuleType
        {
            get
            {
                return ModuleType.Shortcodes;
            }
        }

        [Ignore]
        public int RowId { get; set; }

        [Column("Name")]
        public string Name { get; set; }

        [Column("Description")]
        public string Description { get; set; }

        [Column("AddressCount")]
        public int AddressCount { get; set; }

        public override string ToString()
        {
            return $"[ShortcodePreview: Id={Id}, RowId={RowId}, Name={Name}]";
        }
    }
}

