using System;
using Android.Content;

namespace Mark5.Mobile.Droid.Ui.Views.AddEdtiContactViews
{
    public class LedgerView : AbstractStringSingleRowView
    {
        public LedgerView(Context context)
            : base(context, Resource.String.edit_contact_ledger)
        {
        }

        public override void RefreshView()
        {
            if (!string.IsNullOrEmpty(Contact.Ledger))
                AddRow(Contact.Ledger);
        }

        protected override void Row_DeleteClicked(object sender, EventArgs e)
        {
            Contact.Ledger = string.Empty;
            RemoveRow(sender as Row);
        }

        protected override void TextChanged(string newText)
        {
            Contact.Ledger = newText;
        }
    }
}