//
// Project: Mark5.Mobile.Common
// File: GuidValue.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using SQLite;

#pragma warning disable CS1701
namespace Mark5.Mobile.Common.Model
{
    
    public class GuidValue
    {

        [Column("Guid")]
        public Guid Guid { get; set; }
    }
}
