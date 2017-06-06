using TinyMessenger;

namespace Mark5.Mobile.IOS.Model.HubMessages
{
    public class DocumentPreviewCommentsCountChangedMessage : TinyMessageBase
    {
        public int DocumentPreviewId { get; private set; }

        public int CommentsCount { get; private set; }

        public DocumentPreviewCommentsCountChangedMessage(object sender, int documentId, int commentCount)
            : base(sender)
        {
            DocumentPreviewId = documentId;
            CommentsCount = commentCount;
        }
    }
}