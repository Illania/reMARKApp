using TinyMessenger;

namespace Mark5.Mobile.Droid.Model.HubMessages
{
    public class DocumentPreviewCommentCountChangedMessage : TinyMessageBase
    {
        public int DocumentPreviewId { get; }

        public int CommentsCount { get; }

        public DocumentPreviewCommentCountChangedMessage(object sender, int documentId, int commentCount)
            : base(sender)
        {
            DocumentPreviewId = documentId;
            CommentsCount = commentCount;
        }
    }
}