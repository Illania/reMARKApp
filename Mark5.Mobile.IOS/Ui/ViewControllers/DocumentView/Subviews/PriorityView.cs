using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.DocumentView.Subviews
{
    public class PriorityView : TextSubView
    {
        public PriorityView()
            : base(Localization.GetString("priority"))
        {
        }

        public override void RefreshView()
        {
            if (DocumentPreview != null)
                TextView.Text = UI.PrettyPriorityString(DocumentPreview.Priority);
        }

        public override void UpdateVisibility()
        {
            if (DocumentPreview == null)
            {
                Hidden = true;
                return;
            }

            Hidden = DocumentPreview.Priority != Priority.Low && DocumentPreview.Priority != Priority.Urgent;
        }
    }
}