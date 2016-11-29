//
// Project: Mark5.Mobile.Common
// File: Comment.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;

#pragma warning disable CS1701
namespace Mark5.Mobile.Common.Model
{

    public class Comment : ICopiable<Comment>
    {

        public int Id { get; set; } = -1;

        public Guid Guid { get; set; }

        public long DateAddedTimestamp { get; set; } = -1;

        public int UserId { get; set; }

        public string UserName { get; set; }

        public string Content { get; set; }

        public int ParentId { get; set; }

        public int ParentTypeId { get; set; }

        public Comment ShallowCopy()
        {
            return new Comment
            {
                Id = Id,
                Guid = Guid,
                DateAddedTimestamp = DateAddedTimestamp,
                UserId = UserId,
                UserName = UserName,
                Content = Content,
                ParentId = ParentId,
                ParentTypeId = ParentTypeId,
            };
        }

        public Comment DeepCopy()
        {
            return ShallowCopy();
        }
    }
}

