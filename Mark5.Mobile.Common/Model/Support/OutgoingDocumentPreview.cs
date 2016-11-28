//
// Project: Mark5.Mobile.Common
// File: OutgoingDocumentPreview.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//
using System;
using SQLite;

namespace Mark5.Mobile.Common.Model
{
    public class OutgoingDocumentPreview : DocumentPreview
    {
        [Ignore]
        public OutgoingDocumentState State //We have the same in the container, what to do....?
        {
            get;
            set;
        }

        [Ignore]
        public Guid Identifier
        {
            get;
            set;
        }

        public OutgoingDocumentPreview(DocumentPreview documentPreview)
        {
            this.Addresses = documentPreview.Addresses;
            this.AttachmentsCount = documentPreview.AttachmentsCount;
            this.DateReceivedTimestamp = documentPreview.DateReceivedTimestamp; //When the document was put in the queue
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
        Locked
    }
}
