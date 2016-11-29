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

        [Column("CategoriesBytes")]
        public byte[] CategoriesBytes { get; set; }

        [Ignore]
        public List<Category> Categories
        {
            get
            {
                return SerializationUtils.DeserializeFromByteArray<List<Category>>(CategoriesBytes);
            }
            set
            {
                CategoriesBytes = SerializationUtils.SerializeToByteArray(value);
            }
        }
    }
}

