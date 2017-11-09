using TinyMessenger;

namespace Mark5.Mobile.Common.Model.HubMessages
{
    public class EntityPreviewChangedMessage : TinyMessageBase
    {
        public BusinessEntityPreview EntityPreview { get; }

        public EntityPreviewChangedMessage(object sender, BusinessEntityPreview entityPreview)
            : base(sender)
        {
            EntityPreview = entityPreview;
        }
    }
}
