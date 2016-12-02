
using System;

#pragma warning disable CS1701
namespace Mark5.Mobile.Common.Model
{

    public class OutgoingDocumentInfo
    {
        public Guid Identifier { get; set; }

        public DocumentCreationModeFlag Flag { get; set; }

        public int PreviousDocumentId { get; set; }

        public int PreviousDocumentdFolderId { get; set; }

        public long SendOnTimestamp { get; set; } = -1;

        public bool ConfirmRead { get; set; }

        public bool ConfirmDelivery { get; set; }

        public bool Locked { get; set; }

        public long DateLastSavedTimestamp { get; set; }

        public OutgoingDocumentState State { get; set; }
    }

    public enum OutgoingDocumentState
    {
        None,
        Waiting,
        Failed,
        Sending,
    }
}

