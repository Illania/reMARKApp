using Android.Content;
using Android.Views;

namespace Mark5.Mobile.Droid.Ui.Views.ContactViews
{
    public class LedgerSubview : DescriptionCardSubview
    {
        public LedgerSubview(Context context)
            : base(context)
        {
            Title = context.GetString(Resource.String.ledger);
        }

        public override void RefreshView()
        {
            if (!string.IsNullOrEmpty(Contact?.Ledger))
            {
                Visibility = ViewStates.Visible;
                Content = Contact.Ledger;
            }
            else
            {
                Visibility = ViewStates.Gone;
            }
        }
    }
}