using System;
using Android.Content;

namespace Mark5.Mobile.Droid.Ui.Views.AddEdtiContactViews
{
    public class DescriptionView : AbstractStringSingleRowView
    {
        public DescriptionView(Context context)
            : base(context, Resource.String.edit_contact_description)
        {
        }

        public override void RefreshView()
        {
            if (!string.IsNullOrEmpty(ContactPreview.Description))
                AddRow(ContactPreview.Description);
        }

        protected override void Row_DeleteClicked(object sender, EventArgs e)
        {
            ContactPreview.Description = string.Empty;
            RemoveRow(sender as Row);
        }

        protected override void TextChanged(string newText)
        {
            ContactPreview.Description = newText;
        }
    }
}