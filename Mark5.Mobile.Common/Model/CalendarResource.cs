//
// Project: Mark5.Mobile.Common
// File: CalendarResource.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;

#pragma warning disable CS1701
namespace Mark5.Mobile.Common.Model
{

    public class CalendarResource
    {

        public int Id { get; set; } = -1;

        public Guid Guid { get; set; }

        public string Name { get; set; }

        public string ColorHex { get; set; }

        public bool Shared { get; set; }
    }
}

