using Android.Content;
using Android.Text;

namespace Mark5.Mobile.Droid.Ui.Views.AddEdtiContactViews
{
    public class AccountView : AbstractSimpleFieldView
    {
        public AccountView(Context context)
            : base(context, Resource.String.edit_contact_account, true)
        {
        }

        public override void RefreshView()
        {
            Content = Contact.Account;
        }

        protected override void ContentChanged(object sender, TextChangedEventArgs e)
        {
            Contact.Account = Content;
        }
    }
}
