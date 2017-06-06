using System.Collections.Generic;
using Mark5.Mobile.Common.Model;
using TinyMessenger;

namespace Mark5.Mobile.Droid.Ui.Common.HubMessages
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