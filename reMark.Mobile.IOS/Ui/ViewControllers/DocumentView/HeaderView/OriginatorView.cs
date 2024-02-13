using System.Linq;
using System.Threading.Tasks;
using reMark.Mobile.IOS.Ui.Common;

namespace reMark.Mobile.IOS.Ui.ViewControllers.DocumentView.HeaderView
{
    public class OriginatorView : TextSubView
    {
        public OriginatorView()
            : base(Localization.GetString("originator"))
        {
        }

        public override async Task RefreshView()
        {
            if (Document != null)
                TextView.Text = Document.Lines != null ? string.Join(", ", Document.Lines.Select(l => l.Name).OrderBy(n => n)) : string.Empty;
        }

        public override void UpdateVisibility()
        {
            if (Document == null)
            {
                Hidden = true;
                return;
            }

            Hidden = string.IsNullOrWhiteSpace(Document.Lines != null ? string.Join(", ", Document.Lines.Select(l => l.Name)) : string.Empty);
        }
    }
}
