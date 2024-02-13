using System.Threading.Tasks;
using reMark.Mobile.Common.Model;
using reMark.Mobile.IOS.Ui.Common;

namespace reMark.Mobile.IOS.Ui.ViewControllers.DocumentView.HeaderView
{
    public class CreatorView : TextSubView
    {
        public CreatorView()
            : base(Localization.GetString("creator"))
        {
        }

        public override async Task RefreshView()
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
