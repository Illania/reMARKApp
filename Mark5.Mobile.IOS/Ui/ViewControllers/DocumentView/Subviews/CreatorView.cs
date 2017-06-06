using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.DocumentView.Subviews
{
    public class CreatorView : TextSubView
    {
        public CreatorView()
            : base(Localization.GetString("creator"))
        {
        }

        public override void RefreshView()
        {
            if (DocumentPreview != null)
                TextView.Text = DocumentPreview.Direction == DocumentDirection.Outgoing ? DocumentPreview.Creator : string.Empty;
        }

        public override void UpdateVisibility()
        {
            if (DocumentPreview == null)
            {
                Hidden = true;
                return;
            }

            Hidden = string.IsNullOrWhiteSpace(DocumentPreview.Direction == DocumentDirection.Outgoing ? DocumentPreview.Creator : string.Empty);
        }
    }
}