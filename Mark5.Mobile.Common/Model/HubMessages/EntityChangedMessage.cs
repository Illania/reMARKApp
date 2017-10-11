using TinyMessenger;

namespace Mark5.Mobile.Common.Model.HubMessages
{
    public class EntityChangedMessage : TinyMessageBase
    {
        public ObjectType ObjectType { get; }

        public int EntityId { get; }

        public EntityChangedMessage(object sender, ObjectType objectType, int entityId)
            : base(sender)
        {
            ObjectType = objectType;
            EntityId = entityId;
        }
    }
}