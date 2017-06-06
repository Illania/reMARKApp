//
// File: Line.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//

using System;

namespace Mark5.Mobile.Common.Model
{
    public class Line
    {
        public Guid Guid { get; set; }
        public string Name { get; set; }
        public string FromAddress { get; set; }
    }
}