using TinyMessenger;

namespace Mark5.Mobile.IOS.Model.HubMessages
{
    public class ContactPreviewCommentsCountChangedMessage : TinyMessageBase
    {
        public int ContactPreviewId { get; }

        public int CommentsCount { get; }

        public ContactPreviewCommentsCountChangedMessage(object sender, int contactId, int commentCount)
            : base(sender)
        {
            ContactPreviewId = contactId;
            CommentsCount = commentCount;
        }
    }
}