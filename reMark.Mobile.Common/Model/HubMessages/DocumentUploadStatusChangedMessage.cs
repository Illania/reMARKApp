using System;
using TinyMessenger;

namespace reMark.Mobile.Common.Model.HubMessages
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
            DocumentDelayed,
            DocumentSendCancelled
        }

        public bool IsDraft { get; }

        public Status Change { get; }
        public Guid DocumentGuid { get; }

        public DocumentUploadStatusChangedMessage(object sender, Status change, Guid documentGuid, bool isDraft = false)
            : base(sender)
        {
            Change = change;
            DocumentGuid = documentGuid;
            IsDraft = isDraft;
        }
    }
}