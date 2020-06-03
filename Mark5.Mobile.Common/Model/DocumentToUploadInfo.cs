using System;

namespace Mark5.Mobile.Common.Model
{
    public class DocumentToUploadInfo
    {
        public DocumentCreationModeFlag DocumentCreationModeFlag { get; set; }
        public int? PreviousDocumentId { get; set; } = -1;
        public int? PreviousDocumentFolderId { get; set; } = -1;
        public long SendOnTimestamp { get; set; } = -1;
        public bool ConfirmRead { get; set; }
        public bool ConfirmDelivery { get; set; }
        public DateTime SendDateTime { get; set; }
        public IEventReply IEventReply { get; set; }
    }
}