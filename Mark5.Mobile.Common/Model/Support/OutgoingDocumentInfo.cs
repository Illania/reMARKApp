using System;

namespace Mark5.Mobile.Common.Model
{
    public class OutgoingDocumentInfo
    {
        public Guid Identifier { get; set; }
        public DocumentCreationModeFlag Flag { get; set; }
        public int PrecedingDocumentId { get; set; }
        public int PrecedingDocumentFolderId { get; set; }
        public DateTime SendOn { get; set; }
        public bool ConfirmRead { get; set; }
        public bool ConfirmDelivery { get; set; }
    }
}

