using System.Collections.Generic;
using Mark5.Mobile.Common.Model;
using TinyMessenger;

namespace Mark5.Mobile.Droid.Ui.Common.HubMessages
{
    public class EntityMovedFromFolderMessage : TinyMessageBase
    {
        public ObjectType ObjectType { get; }

        public int FromFolderId { get; }

        public List<int> EntitiesId { get; }

        public EntityMovedFromFolderMessage(object sender, ObjectType objectType, int fromFolderId, List<int> entityId)
            : base(sender)
        {
            ObjectType = objectType;
            FromFolderId = fromFolderId;
            EntitiesId = entityId;
        }
    }
}