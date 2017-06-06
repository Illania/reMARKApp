using System;
using System.Collections.Generic;
using Mark5.Mobile.Common.Model;
using TinyMessenger;

namespace Mark5.Mobile.IOS.Model.HubMessages
{
    public class EntityCategoriesChangedMessage : TinyMessageBase
    {
        public int EntityId { get; }

        public ObjectType ObjectType { get; }

        public List<Category> Categories { get; }

        public EntityCategoriesChangedMessage(object sender, int entityId, ObjectType objectType, List<Category> categories)
            : base(sender)
        {
            EntityId = entityId;
            ObjectType = objectType;
            Categories = categories;
        }
    }
}