//
// Project: Mark5.Mobile.Common
// File: ObjectLinkTypeInfo.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;

namespace Mark5.Mobile.Common.Model
{

    public class ObjectLinkTypeInfo
    {

        public int Id { get; set; } = -1;

        public Guid Guid { get; set; }

        public ObjectType FromType { get; set; }

        public ObjectType ToType { get; set; }

        public string DescriptionSimple { get; set; }

        public string DescriptionComplex { get; set; }

        public string DescriptionComplexReverse { get; set; }

        public string DescriptionAction { get; set; }

        public string DescriptionActionReverse { get; set; }
    }
}

