namespace Mark5.Mobile.Common.Model
{
    public class DocumentWorkingCopy
    {
        public DocumentCreationModeFlag DocumentCreationModeFlag { get; set; }
        public CopyToNewOption CopyToNewOption { get; set; }
        public int? PreviousDocumentId { get; set; }
        public int? PreviousDocumentFolderId { get; set; }
        public DocumentDirection PreviousDocumentDirection { get; set; }
        public long SendOnTimestamp { get; set; } = -1;
        public bool ConfirmRead { get; set; }
        public bool ConfirmDelivery { get; set; }
        public DocumentPreview DocumentPreview { get; set; }
        public Document Document { get; set; }
        public IEventReply IEventReply { get; set; }
    }
}