using TinyMessenger;

namespace Mark5.Mobile.Droid.Ui.Common.HubMessages
{
    public class DocumentPreviewCommentCountChangedMessage : TinyMessageBase
    {
        public int DocumentPreviewId { get; private set; }

        public int CommentsCount { get; private set; }

        public DocumentPreviewCommentCountChangedMessage(object sender, int documentId, int commentCount)
            : base(sender)
        {
            DocumentPreviewId = documentId;
            CommentsCount = commentCount;
        }
    }
}