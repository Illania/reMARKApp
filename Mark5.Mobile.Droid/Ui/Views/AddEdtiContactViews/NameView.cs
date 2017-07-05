using System;
using Android.Content;

namespace Mark5.Mobile.Droid.Ui.Views.AddEdtiContactViews
{
    //To be used with deparment and companies
    public class NameView : AbstractStringSingleRowView
    {
        public NameView(Context context)
            : base(context, Resource.String.edit_contact_name)
        {
        }

        public override void RefreshView()
        {
            if (!string.IsNullOrEmpty(ContactPreview.Name))
                AddRow(ContactPreview.Name);
        }

        protected override void Row_DeleteClicked(object sender, EventArgs e)
        {
            ContactPreview.Name = string.Empty;
            RemoveRow(sender as Row);
        }

        protected override void TextChanged(string newText)
        {
            ContactPreview.Name = newText;
        }

    }
}
