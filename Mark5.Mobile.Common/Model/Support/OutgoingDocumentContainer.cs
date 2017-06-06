using System;
using System.Collections.Generic;

namespace Mark5.Mobile.Common.Model
{
    public class OutgoingDocumentContainer
    {
        Document document;

        public Document Document
        {
            get
            {
                if (LoadMode != LoadMode.Complete)
                    throw new InvalidOperationException($"The document is not set in LoadMode:{LoadMode}!");

                return document;
            }
            set => document = value;
        }

        List<OutgoingDocumentAttachmentDescription> localAttachments = new List<OutgoingDocumentAttachmentDescription>();

        public List<OutgoingDocumentAttachmentDescription> LocalAttachments
        {
            get
            {
                if (LoadMode != LoadMode.Complete)
                    throw new InvalidOperationException($"The attachments are not set in LoadMode:{LoadMode}!");

                return localAttachments;
            }
            set => localAttachments = value;
        }

        public DocumentPreview DocumentPreview { get; set; }
        public OutgoingDocumentInfo Info { get; set; }
        public LoadMode LoadMode { get; set; }
    }

    public enum LoadMode
    {
        Preview,
        Complete,
    }
}