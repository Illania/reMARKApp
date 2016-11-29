//
// Project: Mark5.Mobile.Common
// File: Category.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using SQLite;
using System;

#pragma warning disable CS1701
namespace Mark5.Mobile.Common.Model
{

    [Table("Category")]
    public class Category
    {

        [Column("Id"), PrimaryKey]
        public int Id { get; set; } = -1;

        [Column("Guid")]
        public Guid Guid { get; set; }

        [Column("Name")]
        public string Name { get; set; }

        [Column("Description")]
        public string Description { get; set; }

        [Column("HexColor")]
        public string HexColor { get; set; }

        public override string ToString()
        {
            return $"[Category: Id={Id}, Name={Name}]";
        }
    }
}

