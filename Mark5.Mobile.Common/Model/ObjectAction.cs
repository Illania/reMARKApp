//
// File: ObjectAction.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//

using System;

namespace Mark5.Mobile.Common.Model
{
    public class ObjectAction
    {
        public int Id { get; set; } = -1;

        public string ActionType { get; set; }
        public Guid ActionTypeGid { get; set; }
        public int ActionTypeId { get; set; } = -1;

        public int UserId { get; set; } = -1;

        public string Username { get; set; }
        public string Description { get; set; }
        public long ActionTimeTimestamp { get; set; } = -1;
    }
}