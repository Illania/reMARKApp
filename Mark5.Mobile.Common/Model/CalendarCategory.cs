//
// Project: Mark5.Mobile.Common
// File: CalendarCategory.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;

namespace Mark5.Mobile.Common.Model
{

    public class CalendarCategory
    {

        public int Id { get; set; }

        public Guid Guid { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string ColorHex { get; set; }

        public CalendarCategoryType Type { get; set; }

        public CalendarCategorySubType SubType { get; set; }
    }
}

