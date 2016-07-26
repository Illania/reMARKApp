//
// Project: Mark5.Mobile.Common
// File: ObjectLink.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//

namespace Mark5.Mobile.Common.Model
{

    public class ObjectLink
    {

        public int FromObjectId { get; set; } = -1;

        public ObjectType FromObjectType { get; set; }

        public int ToObjectId { get; set; } = -1;

        public ObjectType ToObjectType { get; set; }

        public bool IsReverse { get; set; }

        public string Description { get; set; }

        public ObjectLinkTypeInfo TypeInfo { get; set; }
    }
}

