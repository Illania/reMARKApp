using Android.Content;
using Android.Views;

namespace Mark5.Mobile.Droid.Ui.Views.ContactViews
{
    public class AccountSubview : DescriptionCardSubview
    {
        public AccountSubview(Context context)
            : base(context)
        {
            Title = context.GetString(Resource.String.account);
        }

        public override void RefreshView()
        {
            if (!string.IsNullOrEmpty(Contact?.Account))
            {
                Visibility = ViewStates.Visible;
                Content = Contact.Account;
            }
            else
            {
                Visibility = ViewStates.Gone;
            }
        }
    }
}