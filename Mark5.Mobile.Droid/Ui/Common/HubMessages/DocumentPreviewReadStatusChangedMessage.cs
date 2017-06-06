using TinyMessenger;

namespace Mark5.Mobile.Droid.Ui.Common.HubMessages
{
    public class DocumentPreviewReadStatusChangedMessage : TinyMessageBase
    {
        public int DocumentPreviewId { get; private set; }

        public bool IsReadByCurrent { get; private set; }

        public bool IsReadByAnyone { get; private set; }

        public DocumentPreviewReadStatusChangedMessage(object sender, int documentPreviewId, bool isReadByCurrent, bool isReadByAnyone)
            : base(sender)
        {
            DocumentPreviewId = documentPreviewId;
            IsReadByCurrent = isReadByCurrent;
            IsReadByAnyone = isReadByAnyone;
        }
    }
}