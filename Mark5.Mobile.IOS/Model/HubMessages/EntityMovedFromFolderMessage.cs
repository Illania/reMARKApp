using System.Collections.Generic;
using Mark5.Mobile.Common.Model;
using TinyMessenger;

namespace Mark5.Mobile.IOS.Model.HubMessages
{
    public class EntityMovedFromFolderMessage : TinyMessageBase
    {
        public ObjectType ObjectType { get; private set; }

        public int FromFolderId { get; private set; }

        public List<int> EntitiesId { get; private set; }

        public EntityMovedFromFolderMessage(object sender, ObjectType objectType, int fromFolderId, List<int> entityId)
            : base(sender)
        {
            ObjectType = objectType;
            FromFolderId = fromFolderId;
            EntitiesId = entityId;
        }
    }
}