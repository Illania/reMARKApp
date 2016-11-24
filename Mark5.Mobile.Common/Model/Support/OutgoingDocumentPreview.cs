//
// Project: Mark5.Mobile.Common
// File: OutgoingDocumentPreview.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//
using SQLite;

namespace Mark5.Mobile.Common.Model.Support
{
    public class OutgoingDocumentPreview : DocumentPreview
    {
        [Ignore]
        public OutgoingDocumentState State
        {
            get;
            set;
        }

        public OutgoingDocumentPreview(DocumentPreview documentPreview)
        {
            this.Addresses = documentPreview.Addresses;
            this.AttachmentsCount = documentPreview.AttachmentsCount;
            this.DateReceivedTimestamp = documentPreview.DateReceivedTimestamp;
            this.Direction = documentPreview.Direction;
            this.Preview = documentPreview.Preview;
            this.Priority = documentPreview.Priority;
            this.Subject = documentPreview.Subject;
        }
    }

    public enum OutgoingDocumentState
    {
        Waiting,
        Failed,
        Sending,
    }
}
