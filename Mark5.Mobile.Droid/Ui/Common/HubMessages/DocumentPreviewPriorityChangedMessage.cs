using System.Collections.Generic;
using Mark5.Mobile.Common.Model;
using TinyMessenger;

namespace Mark5.Mobile.Droid.Ui.Common.HubMessages
{
    public class DocumentPreviewCategoriesChangedMessage : TinyMessageBase
    {
        public int DocumentPreviewId { get; private set; }

        public List<Category> Categories { get; private set; }

        public DocumentPreviewCategoriesChangedMessage(object sender, int documentPreviewId, List<Category> categories)
            : base(sender)
        {
            DocumentPreviewId = documentPreviewId;
            Categories = categories;
        }
    }
}