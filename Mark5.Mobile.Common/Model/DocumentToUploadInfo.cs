namespace Mark5.Mobile.Common.Model
{
    public class DocumentToUploadInfo
    {
        public DocumentCreationModeFlag CreationModeFlag { get; set; }
        public int PreviousDocumentId { get; set; } = -1;
        public int PreviousDocumentdFolderId { get; set; } = -1;
        public long SendOnTimestamp { get; set; } = -1;
        public bool ConfirmRead { get; set; }
        public bool ConfirmDelivery { get; set; }
    }
}