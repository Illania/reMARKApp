using TinyMessenger;

namespace reMark.Mobile.Common.Model.HubMessages
{
    public class DocumentPreviewReadStatusChangedMessage : TinyMessageBase, IMessageWithId
    {
        public int Id => DocumentPreviewId;
        public int DocumentPreviewId { get; }

        public bool IsReadByCurrent { get; }

        public bool IsReadByAnyone { get; }

        public DocumentPreviewReadStatusChangedMessage(object sender, int documentPreviewId, bool isReadByCurrent, bool isReadByAnyone)
            : base(sender)
        {
            DocumentPreviewId = documentPreviewId;
            IsReadByCurrent = isReadByCurrent;
            IsReadByAnyone = isReadByAnyone;
        }
    }
}