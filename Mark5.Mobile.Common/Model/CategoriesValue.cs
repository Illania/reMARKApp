//
// Project: Mark5.Mobile.Common
// File: CategoriesValue.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System.Collections.Generic;
using Mark5.Mobile.Common.Utilities;
using SQLite;

#pragma warning disable CS1701
namespace Mark5.Mobile.Common.Model
{

    public class CategoriesValue
    {

        [Column("CategoriesString")]
        public string CategoriesString { get; set; }

        [Ignore]
        public List<Category> Categories
        {
            get
            {
                return SerializationUtils.Deserialize<List<Category>>(CategoriesString);
            }
            set
            {
                CategoriesString = SerializationUtils.Serialize(value);
            }
        }
    }
}

