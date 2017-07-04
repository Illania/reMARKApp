using System;
using Android.Content;

namespace Mark5.Mobile.Droid.Ui.Views.AddEdtiContactViews
{
    public class LastNameView : AbstractStringSingleRowView
    {
        public LastNameView(Context context)
            : base(context, Resource.String.edit_contact_last_name)
        {
        }

        public override void RefreshView()
        {
            if (!string.IsNullOrEmpty(Contact.LastName))
                AddRow(Contact.LastName);
        }

        protected override void Row_DeleteClicked(object sender, EventArgs e)
        {
            Contact.LastName = string.Empty;
            RemoveRow(sender as Row);
        }

        protected override void TextChanged(string newText)
        {
            Contact.LastName = newText;
        }
    }
}
