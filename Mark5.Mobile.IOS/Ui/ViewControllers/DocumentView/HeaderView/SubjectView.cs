namespace Mark5.Mobile.IOS.Ui.ViewControllers.DocumentView.HeaderView
{
    public class SubjectView : LargeTextSubView
    {
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
