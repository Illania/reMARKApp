using TinyMessenger;

namespace Mark5.Mobile.Common.Model.HubMessages
{
    public class EntityChangedMessage : TinyMessageBase
    {
        public BusinessEntity Entity { get; }

        public EntityChangedMessage(object sender, BusinessEntity entity)
            : base(sender)
        {
            Entity = entity;
        }
    }
}