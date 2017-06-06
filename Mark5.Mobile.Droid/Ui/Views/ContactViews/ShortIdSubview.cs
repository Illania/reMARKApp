using Android.Content;
using Android.Views;

namespace Mark5.Mobile.Droid.Ui.Views.ContactViews
{
    public class ShortIdSubview : DescriptionCardSubview
    {
        public ShortIdSubview(Context context)
            : base(context)
        {
            Title = context.GetString(Resource.String.short_id);
        }

        public override void RefreshView()
        {
            if (!string.IsNullOrWhiteSpace(ContactPreview?.ShortId))
            {
                Visibility = ViewStates.Visible;
                Content = ContactPreview.ShortId;
            }
            else
            {
                Visibility = ViewStates.Gone;
            }
        }
    }
}