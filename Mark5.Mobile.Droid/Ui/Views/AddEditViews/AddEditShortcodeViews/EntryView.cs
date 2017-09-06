using System;
using Android.Content;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Ui.Views.AddEditShortcodeViews
{
    public class EntryView : AbstractMultipleRowsView<DocumentAddress>
    {
        public EntryView(Context context, int titleResourceId)
            : base(context, titleResourceId)
        {
        }

        public override void RefreshView()
        {
            throw new NotImplementedException();
        }

        protected override void AddButton_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        protected override Row GetNewRow()
        {
            throw new NotImplementedException();
        }

        protected override void Row_DeleteClicked(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
