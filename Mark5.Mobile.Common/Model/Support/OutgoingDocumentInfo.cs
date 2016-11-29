
using System;

namespace Mark5.Mobile.Common.Model
{

    public class OutgoingDocumentInfo
    {

        public Guid Identifier { get; set; }

        public DocumentCreationModeFlag Flag { get; set; }

        public int PrecedingDocumentId { get; set; }

        public int PrecedingDocumentFolderId { get; set; }

        public long SendOnTimestamp { get; set; } = -1;

        public bool ConfirmRead { get; set; }

        public bool ConfirmDelivery { get; set; }

        public OutgoingDocumentState State { get; set; }

        public bool Locked { get; set; }
    }

    public enum OutgoingDocumentState
    {
        Waiting,
        Failed,
        Sending,
    }
}

