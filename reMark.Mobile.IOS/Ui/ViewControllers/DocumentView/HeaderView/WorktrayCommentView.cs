using System;
using System.Threading.Tasks;
using reMark.Mobile.IOS.Ui.Common;

namespace reMark.Mobile.IOS.Ui.ViewControllers.DocumentView.HeaderView
{
    public class WorktrayCommentView : TextSubView
    {
        public WorktrayCommentView()
            : base(Localization.GetString("worktray_comment"))
        {
        }

        public override async Task RefreshView()
        {
            if (!string.IsNullOrEmpty(Document.WorktrayComment) && TextView != null)
                TextView.Text = Document.WorktrayComment;
        }

        public override void UpdateVisibility()
        {
            if (Document == null || string.IsNullOrEmpty(Document.WorktrayComment))
            {
                Hidden = true;
                return;
            }
            

            Hidden = Document.ReadByUserNames.Count < 1;
        }
    }
}
