using Mark5.Mobile.IOS.Ui.Common;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.DocumentView.Subviews
{
    public class SubjectView : LargeTextSubView
    {
        public SubjectView()
        {
        }

        public override void RefreshView()
        {
            if (DocumentPreview != null)
                TextView.Text = DocumentPreview.Subject;
        }

        public override void UpdateVisibility()
        {
            if (DocumentPreview == null)
            {
                Hidden = true;
                return;
            }

            Hidden = string.IsNullOrWhiteSpace(DocumentPreview.Subject);
        }
    }
}