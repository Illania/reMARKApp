using System;
using Android.Content;

namespace reMark.Mobile.Droid.Ui.Views.AddEditContactViews
{
    public class LedgerView : AbstractSimpleFieldView
    {
        public LedgerView(Context context)
            : base(context, Resource.String.edit_contact_ledger, true)
        {
        }

        public override void RefreshView()
        {
            Content = Contact.Ledger;
        }

        protected override void ContentChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            Contact.Ledger = Content;
        }

    }
}