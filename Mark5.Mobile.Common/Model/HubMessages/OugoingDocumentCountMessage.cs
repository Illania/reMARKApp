using System;
using TinyMessenger;

namespace Mark5.Mobile.Common.Model.HubMessages
{
    /*
     *   Used to message regards pending/failed documents
     */
    public class OugoingDocumentCountMessage : TinyMessageBase
    {
        public int PendingCount { get; } = 0;
        public bool HasFailedDocuments { get; }

        public OugoingDocumentCountMessage(object sender, int count, bool hasFailedDocuments)
            : base(sender)
        {
            PendingCount = count;
            HasFailedDocuments = hasFailedDocuments;
        }
    }
}
