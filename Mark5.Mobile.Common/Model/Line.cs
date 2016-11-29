//
// Project: Mark5.Mobile.Common
// File: Line.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;

#pragma warning disable CS1701
namespace Mark5.Mobile.Common.Model
{

    public class Line
    {

        public Guid Guid { get; set; }

        public string Name { get; set; }

        public string FromAddress { get; set; }
    }
}

