//
// Project: Mark5.Mobile.IOS
// File: EntityCategoriesChangedMessage.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//

using System;
using System.Collections.Generic;
using Mark5.Mobile.Common.Model;
using TinyMessenger;

namespace Mark5.Mobile.IOS.Model.HubMessages
{
    public class EntityCategoriesChangedMessage : TinyMessageBase
    {
        public int EntityId { get; private set; }

        public ObjectType ObjectType { get; private set; }

        public List<Category> Categories { get; private set; }

        public EntityCategoriesChangedMessage(object sender, int entityId, ObjectType objectType, List<Category> categories)
            : base(sender)
        {
            EntityId = entityId;
            ObjectType = objectType;
            Categories = categories;
        }
    }
}