using System.Collections.Generic;
using TinyMessenger;

namespace reMark.Mobile.Common.Model.HubMessages
{
    public class EntityRemovedMessage : TinyMessageBase
    {
        public ObjectType ObjectType { get; }

        public List<int> EntitiesId { get; }

        public EntityRemovedMessage(object sender, ObjectType objectType, List<int> entityId)
            : base(sender)
        {
            ObjectType = objectType;
            EntitiesId = entityId;
        }
    }
}
