using System;
using TinyMessenger;

namespace Mark5.Mobile.Common.Model.HubMessages
{
    public class DocumentUploadStatusChangedMessage : TinyMessageBase
    {
        public enum Status
        {
            None,
            DocumentSending,
            DocumentSent,
            DocumentSentFailed,
            DocumentDiscarded,
        }

        public Status Change { get; }
        public Guid DocumentGuid { get; }

        public DocumentUploadStatusChangedMessage(object sender, Status change, Guid documentGuid)
            : base(sender)
        {
            Change = change;
            DocumentGuid = documentGuid;
        }
    }
}