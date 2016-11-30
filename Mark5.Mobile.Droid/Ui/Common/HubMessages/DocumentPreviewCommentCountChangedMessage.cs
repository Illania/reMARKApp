//
// Project: Mark5.Mobile.Droid
// File: DocumentPreviewCommentCountChangedMessage.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//
using TinyMessenger;

namespace Mark5.Mobile.Droid.Ui.Common.HubMessages
{

    public class DocumentPreviewCommentCountChangedMessage : TinyMessageBase
    {

        public int DocumentPreviewId
        {
            get;
            private set;
        }

        public int CommentsCount
        {
            get;
            private set;
        }

        public DocumentPreviewCommentCountChangedMessage(object sender, int documentId, int commentCount) : base(sender)
        {
            DocumentPreviewId = documentId;
            CommentsCount = commentCount;
        }
    }
}
