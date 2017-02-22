//
// Project: ${Project}
// File: CommentsCountChangeMessage.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using Mark5.Mobile.Common.Model;
using TinyMessenger;

namespace Mark5.Mobile.IOS.Ui.Common.HubMessages
{
    public class CommentsCountChangeMessage : TinyMessageBase
    {
        public ObjectType ObjectType
        {
            get;
            private set;
        }

        public int EntityId
        {
            get;
            private set;
        }

        public int CommentsCount
        {
            get;
            private set;
        }

        public CommentsCountChangeMessage(object sender, ObjectType objectType, int entityId, int commentsCount) : base(sender)
        {
            ObjectType = objectType;
            EntityId = entityId;
            CommentsCount = commentsCount;
        }
    }
}
