using Android.Content;
using Android.Views;

namespace Mark5.Mobile.Droid.Ui.Views.ContactViews
{
    public class VatSubview : DescriptionCardSubview
    {
        public VatSubview(Context context)
            : base(context)
        {
            Title = context.GetString(Resource.String.vat);
        }

        public override void RefreshView()
        {
            if (!string.IsNullOrWhiteSpace(Contact?.Vat))
            {
                Visibility = ViewStates.Visible;
                Content = Contact.Vat;
            }
            else
            {
                Visibility = ViewStates.Gone;
            }
        }
    }
}