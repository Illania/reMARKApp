using System;
using TinyMessenger;

namespace Mark5.Mobile.Common.Model.HubMessages
{
    public class DocumentUploadStatusChanged : TinyMessageBase
    {

        public enum Status
        {
            None,
            DocumentSending,
            DocumentSent,
            DocumentSentFailed
        }

        public Status Change { get; }
        public Guid DocumentGuid { get; }

        public DocumentUploadStatusChanged(object sender, Status change, Guid documentGuid)
            : base(sender)
        {
            Change = change;
            DocumentGuid = documentGuid;
        }
    }
}