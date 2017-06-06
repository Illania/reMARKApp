using System;
using System.Collections.Generic;

namespace Mark5.Mobile.Common.Model
{
    public class SendDocumentInfo
    {
        public Document Document { get; set; }
        public DocumentPreview DocumentPreview { get; set; }
        public DocumentCreationModeFlag CreationModeFlag { get; set; }
        public int PreceedingDocumentId { get; set; } = -1;

        public int PreceedingDocumentFolderId { get; set; } = -1;

        public long SendOnTimestamp { get; set; } = -1;

        public bool ConfirmRead { get; set; }
        public bool ConfirmDelivery { get; set; }
        List<Guid> temporaryAttachmentGuids;

        public List<Guid> TemporaryAttachmentGuids
        {
            get
            {
                if (temporaryAttachmentGuids == null)
                    temporaryAttachmentGuids = new List<Guid>();
                return temporaryAttachmentGuids;
            }
            set => temporaryAttachmentGuids = value;
        }
    }
}