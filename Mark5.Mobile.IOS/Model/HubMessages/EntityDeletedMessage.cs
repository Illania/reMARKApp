using System.Collections.Generic;
using Mark5.Mobile.Common.Model;
using TinyMessenger;

namespace Mark5.Mobile.IOS.Model.HubMessages
{
    public class EntityDeletedMessage : TinyMessageBase
    {
        public ObjectType ObjectType { get; }

        public List<int> EntitiesId { get; }

        public EntityDeletedMessage(object sender, ObjectType objectType, List<int> entitiesId)
            : base(sender)
        {
            ObjectType = objectType;
            EntitiesId = entitiesId;
        }
    }
}