using System;
using Android.Content;

namespace Mark5.Mobile.Droid.Ui.Views.AddEdtiContactViews
{
    public class PositionView : AbstractStringSingleRowView
    {
        public PositionView(Context context)
            : base(context, Resource.String.edit_contact_position)
        {
        }

        public override void RefreshView()
        {
            if (!string.IsNullOrEmpty(Contact.Position))
                AddRow(Contact.Position);
        }

        protected override void Row_DeleteClicked(object sender, EventArgs e)
        {
            Contact.Position = string.Empty;
            RemoveRow(sender as Row);
        }

        protected override void TextChanged(string newText)
        {
            Contact.Position = newText;
        }

    }
}