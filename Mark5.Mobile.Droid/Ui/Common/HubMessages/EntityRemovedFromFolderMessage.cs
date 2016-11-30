//
// Project: Mark5.Mobile.Droid
// File: EntityRemovedFromFolderMessage.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//
using System.Collections.Generic;
using Mark5.Mobile.Common.Model;
using TinyMessenger;

namespace Mark5.Mobile.Droid.Ui.Common.HubMessages
{

    public class EntityRemovedFromFolderMessage : TinyMessageBase
    {

        public ObjectType ObjectType
        {
            get;
            private set;
        }

        public int FromFolderId
        {
            get;
            private set;
        }

        public List<int> EntitiesId
        {
            get;
            private set;
        }

        public EntityRemovedFromFolderMessage(object sender, ObjectType objectType, int fromFolderId, List<int> entityId)
            : base(sender)
        {
            ObjectType = objectType;
            FromFolderId = fromFolderId;
            EntitiesId = entityId;
        }
    }
}
