//
// Project: Mark5.Mobile.Common
// File: IdValue.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using SQLite;

namespace Mark5.Mobile.Common.Model
{
    public class IdValue
    {

        [Column("Id")]
        public int Id { get; set; } = -1;
    }
}

