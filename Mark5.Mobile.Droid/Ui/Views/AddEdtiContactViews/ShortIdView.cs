using System;
using Android.Content;

namespace Mark5.Mobile.Droid.Ui.Views.AddEdtiContactViews
{
    public class ShortIdView : AbstractStringSingleRowView
    {
        public ShortIdView(Context context)
            : base(context, Resource.String.edit_contact_short_id)
        {
        }

        public override void RefreshView()
        {
            if (!string.IsNullOrEmpty(ContactPreview.ShortId))
                AddRow(ContactPreview.ShortId);
        }

        protected override void Row_DeleteClicked(object sender, EventArgs e)
        {
            ContactPreview.ShortId = string.Empty;
            RemoveRow(sender as Row);
        }

        protected override void TextChanged(string newText)
        {
            ContactPreview.ShortId = newText;
        }

    }
}