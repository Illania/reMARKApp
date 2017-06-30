using System.Linq;
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

        public override void UpdateContact()
        {
            string position;
            if (Rows.Any() && !string.IsNullOrEmpty(position = Rows[0].GetContent()))
                Contact.Ledger = position;
        }
    }
}