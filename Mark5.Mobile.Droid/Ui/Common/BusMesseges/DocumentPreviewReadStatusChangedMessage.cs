//
// Project: Mark5.Mobile.Droid
// File: ReadStatusChangedMessage.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using TinyMessenger;

namespace Mark5.Mobile.Droid.Ui.Common.BusMesseges
{

    public class DocumentPreviewReadStatusChangedMessage : TinyMessageBase
    {

        public int FolderId
        {
            get;
            private set;
        }

        public int DocumentPreviewId
        {
            get;
            private set;
        }

        public bool IsReadByCurrent
        {
            get;
            private set;
        }

        public bool IsReadByAnyone
        {
            get;
            private set;
        }

        public DocumentPreviewReadStatusChangedMessage(object sender, int documentPreviewId, bool isReadByCurrent, bool isReadByAnyone)
            : base(sender)
        {
            DocumentPreviewId = documentPreviewId;
            IsReadByCurrent = isReadByCurrent;
            IsReadByAnyone = isReadByAnyone;
        }
    }
}
