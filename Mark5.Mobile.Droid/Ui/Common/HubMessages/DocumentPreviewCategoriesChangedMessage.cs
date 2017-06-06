//
// Project: Mark5.Mobile.Droid
// File: DocumentPreviewPriorityChangedMessage.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//

using Mark5.Mobile.Common.Model;
using TinyMessenger;

namespace Mark5.Mobile.Droid.Ui.Common.HubMessages
{
    public class DocumentPreviewPriorityChangedMessage : TinyMessageBase
    {
        public int DocumentPreviewId { get; private set; }

        public Priority Priority { get; private set; }

        public DocumentPreviewPriorityChangedMessage(object sender, int documentPreviewId, Priority priority)
            : base(sender)
        {
            DocumentPreviewId = documentPreviewId;
            Priority = priority;
        }
    }
}