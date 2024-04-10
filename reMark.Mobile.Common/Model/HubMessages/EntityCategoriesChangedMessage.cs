using TinyMessenger;

namespace reMark.Mobile.Common.Model.HubMessages
{
    public class EntityCategoriesChangedMessage : TinyMessageBase, IMessageWithId
    {
        public int Id => EntityId;
        
        public ObjectType ObjectType { get; }

        public int EntityId { get; }

        public List<Category> Categories { get; }

        public EntityCategoriesChangedMessage(object sender, ObjectType objectType, int entityId, List<Category> categories)
            : base(sender)
        {
            ObjectType = objectType;
            EntityId = entityId;
            Categories = categories;
        }
    }
}
