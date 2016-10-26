//
// Project: Mark5.Mobile.Common
// File: Comment.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;

namespace Mark5.Mobile.Common.Model
{

    public class Comment
    {

        public int Id { get; set; } = -1;

        public Guid Guid { get; set; }

        public long DateAddedTimestamp { get; set; } = -1;

        public int UserId { get; set; }

        public string UserName { get; set; }

        public string Content { get; set; }

        public int ParentId { get; set; }

        public int ParentTypeId { get; set; }
    }
}

