using System.Linq;
using Mark5.Mobile.IOS.Ui.Common;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.DocumentView.HeaderView
{
    public class ReadByView : TextSubView
    {
        public ReadByView()
            : base(Localization.GetString("read_by"))
        {
        }

        public override void RefreshView()
        {
            if (Document != null)
            {
                var readByUsernames = Document.ReadByUserNames.Values.SelectMany(s => s.Split('|')).OrderBy(s => s).Select(s => s.ToUpper());
                TextView.Text = string.Join(", ", readByUsernames);
            }
        }

        public override void UpdateVisibility()
        {
            if (Document == null)
            {
                Hidden = true;
                return;
            }

            Hidden = Document.ReadByUserNames.Count < 1;
        }
    }
}
