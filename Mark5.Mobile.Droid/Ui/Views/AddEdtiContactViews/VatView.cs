using System;
using Android.Content;

namespace Mark5.Mobile.Droid.Ui.Views.AddEdtiContactViews
{
    public class VatView : AbstractStringSingleRowView
    {
        public VatView(Context context)
            : base(context, Resource.String.edit_contact_vat)
        {
        }

        public override void RefreshView()
        {
            if (!string.IsNullOrEmpty(Contact.Vat))
                AddRow(Contact.Vat);
        }

        protected override void Row_DeleteClicked(object sender, EventArgs e)
        {
            Contact.Vat = string.Empty;
            RemoveRow(sender as Row);
        }

        protected override void TextChanged(string newText)
        {
            Contact.Vat = newText;
        }

    }
}