using System;

namespace Mark5.Mobile.Common.Model
{
    public class OutgoingDocumentInfo //TODO need to find a good place for this
    {
        public Guid Identifier { get; set; } //We suppose that this value is created anew for every new document
        public DocumentCreationModeFlag Flag { get; set; }
        public int PrecedingDocumentId { get; set; }
        public int PrecedingDocumentFolderId { get; set; }
        public DateTime SendOn { get; set; }
        public bool ConfirmRead { get; set; }
        public bool ConfirmDelivery { get; set; }
    }

    public class OutgoingDocumentContainer //TODO need to find a good place for this
    {
        public Document Document { get; set; }
        public DocumentPreview DocumentPreview { get; set; }
        public OutgoingDocumentInfo Info { get; set; }
    }
}

