//
// Project: Mark5.Mobile.IOS
// File: EntityMovedFromFolderMessage.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System.Collections.Generic;
using Mark5.Mobile.Common.Model;
using TinyMessenger;

namespace Mark5.Mobile.IOS.Ui.Common.HubMessages
{
    
    public class EntityMovedFromFolderMessage : TinyMessageBase
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

        public EntityMovedFromFolderMessage(object sender, ObjectType objectType, int fromFolderId, List<int> entityId)
            : base(sender)
        {
            ObjectType = objectType;
            FromFolderId = fromFolderId;
            EntitiesId = entityId;
        }
    }
}
