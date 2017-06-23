using System.Collections.Generic;
using Mark5.Mobile.Common.Model;
using TinyMessenger;

namespace Mark5.Mobile.Droid.Model.HubMessages
{
    public class ContactPreviewCategoriesChangedMessage : TinyMessageBase
    {
        public int ContactPreviewId { get; }

        public List<Category> Categories { get; }

        public ContactPreviewCategoriesChangedMessage(object sender, int contactPreviewId, List<Category> categories)
            : base(sender)
        {
            ContactPreviewId = contactPreviewId;
            Categories = categories;
        }
    }
}