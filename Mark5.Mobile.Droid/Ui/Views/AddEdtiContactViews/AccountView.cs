using System;
using Android.Content;

namespace Mark5.Mobile.Droid.Ui.Views.AddEdtiContactViews
{
    public class AccountView : AbstractStringSingleRowView
    {
        public AccountView(Context context)
            : base(context, Resource.String.edit_contact_account)
        {
        }

        public override void RefreshView()
        {
            if (!string.IsNullOrEmpty(Contact.Account))
                AddRow(Contact.Account);
        }

        protected override void Row_DeleteClicked(object sender, EventArgs e)
        {
            Contact.Account = string.Empty;
            RemoveRow(sender as Row);
        }

        protected override void TextChanged(string newText)
        {
            Contact.Account = newText;
        }
    }
}
