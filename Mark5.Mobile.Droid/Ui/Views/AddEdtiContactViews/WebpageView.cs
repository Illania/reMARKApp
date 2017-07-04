using System;
using Android.Content;

namespace Mark5.Mobile.Droid.Ui.Views.AddEdtiContactViews
{
    public class WebpageView : AbstractStringSingleRowView
    {
        public WebpageView(Context context)
            : base(context, Resource.String.edit_contact_webpage)
        {
        }

        public override void RefreshView()
        {
            if (!string.IsNullOrEmpty(Contact.WebPageAddress))
                AddRow(Contact.WebPageAddress);
        }

        protected override void Row_DeleteClicked(object sender, EventArgs e)
        {
            Contact.WebPageAddress = string.Empty;
            RemoveRow(sender as Row);
        }

        protected override void TextChanged(string newText)
        {
            Contact.WebPageAddress = newText;
        }

    }
}