using System;
using TinyMessenger;

namespace Mark5.Mobile.Common.Model.HubMessages
{
    /*
     *   Used to message regards pending/failed documents
     */
    public class OugoingDocumentCountMessage : TinyMessageBase
    {
        public int TotalCount { get; } = -1;
        public bool HasFailedDocuments { get; }

        public OugoingDocumentCountMessage(object sender, int count, bool hasFailedDocuments)
            : base(sender)
        {
            // if the value is -1, we are not displaying anything
            TotalCount = count > 0 ? count : -1;
            HasFailedDocuments = hasFailedDocuments;
        }
    }
}
