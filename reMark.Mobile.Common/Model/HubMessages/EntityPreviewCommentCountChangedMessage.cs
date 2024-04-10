using TinyMessenger;

namespace reMark.Mobile.Common.Model.HubMessages
{
    public class EntityPreviewCommentCountChangedMessage : TinyMessageBase, IMessageWithId
    {
        public int Id => EntityId;
        
        public ObjectType ObjectType { get; }

        public int EntityId { get; }

        public int CommentsCount { get; }

        public EntityPreviewCommentCountChangedMessage(object sender, ObjectType objectType, int documentId, int commentCount)
            : base(sender)
        {
            ObjectType = objectType;
            EntityId = documentId;
            CommentsCount = commentCount;
        }
    }
}
