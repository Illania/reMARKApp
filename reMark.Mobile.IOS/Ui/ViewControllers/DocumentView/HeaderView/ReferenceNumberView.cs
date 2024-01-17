using System;
using System.Threading.Tasks;
using reMark.Mobile.IOS.Ui.Common;

namespace reMark.Mobile.IOS.Ui.ViewControllers.DocumentView.HeaderView
{
    public class ReferenceNumberView : TextSubView
    {
        public ReferenceNumberView()
            : base(Localization.GetString("reference_number"))
        {
        }

        public override async Task RefreshView()
        {
            if (DocumentPreview != null)
                TextView.Text = DocumentPreview.ReferenceNumber;
        }

        public override void UpdateVisibility()
        {
            if (DocumentPreview == null)
            {
                Hidden = true;
                return;
            }

            Hidden = string.IsNullOrWhiteSpace(DocumentPreview.ReferenceNumber);
        }
    }
}
