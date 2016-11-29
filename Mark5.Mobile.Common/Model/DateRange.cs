//
// Project: Mark5.Mobile.Common
// File: DateRange.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;

#pragma warning disable CS1701
namespace Mark5.Mobile.Common.Model
{

    public class DateRange
    {

        public long StartTimestamp { get; set; } = -1;

        public long EndTimestamp { get; set; } = -1;

        public bool Enabled { get; set; }
    }
}

