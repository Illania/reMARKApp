using System.Threading.Tasks;
using reMark.Mobile.Common.Model;
using reMark.Mobile.IOS.Ui.Common;
using reMark.Mobile.IOS.Utilities;

namespace reMark.Mobile.IOS.Ui.ViewControllers.DocumentView.HeaderView
{
    public class PriorityView : TextSubView
    {
        public PriorityView()
            : base(Localization.GetString("priority"))
        {
        }

        public override async Task RefreshView()
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
