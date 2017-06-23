using System;
using TinyMessenger;

namespace Mark5.Mobile.Common.Model.HubMessages
{
    public class DocumentInUploadChangedMessage : TinyMessageBase
    {

        public enum ChangeType
        {
            None,
            DocumentAdded,
            DocumentSending,
            DocumentSent,
            DocumentSentFailed
        }

        public ChangeType Change { get; set; }
        public Guid DocumentGuid { get; }

        public DocumentInUploadChangedMessage(object sender, ChangeType change, Guid documentGuid)
            : base(sender)
        {
            Change = change;
            DocumentGuid = documentGuid;
        }
    }
}