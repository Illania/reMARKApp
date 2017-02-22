//
// Project: ${Project}
// File: CommentsCountChangeMessage.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using Mark5.Mobile.Common.Model;
using TinyMessenger;

namespace Mark5.Mobile.IOS.Ui.Common.HubMessages
{
    public class DocumentPreviewCommentsCountChangedMessage : TinyMessageBase
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

        public DocumentPreviewCommentsCountChangedMessage(object sender, int documentId, int commentCount) : base(sender)
        {
            DocumentPreviewId = documentId;
            CommentsCount = commentCount;
        }
    }
}
