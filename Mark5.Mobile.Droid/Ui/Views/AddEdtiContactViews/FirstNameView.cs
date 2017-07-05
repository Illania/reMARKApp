using System;
using Android.Content;

namespace Mark5.Mobile.Droid.Ui.Views.AddEdtiContactViews
{
    public class FirstNameView : AbstractStringSingleRowView
    {
        public FirstNameView(Context context)
            : base(context, Resource.String.edit_contact_first_name)
        {
        }

        public override void RefreshView()
        {
            if (!string.IsNullOrEmpty(Contact.FirstName))
                AddRow(Contact.FirstName);
        }

        protected override void Row_DeleteClicked(object sender, EventArgs e)
        {
            Contact.FirstName = string.Empty;
            RemoveRow(sender as Row);
        }

        protected override void TextChanged(string newText)
        {
            Contact.FirstName = newText;
        }
    }
}
