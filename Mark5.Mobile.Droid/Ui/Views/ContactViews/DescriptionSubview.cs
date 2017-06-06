using Android.Content;
using Android.Views;

namespace Mark5.Mobile.Droid.Ui.Views.ContactViews
{
    public class DescriptionSubview : DescriptionCardSubview
    {
        public DescriptionSubview(Context context)
            : base(context)
        {
            Title = context.GetString(Resource.String.description);
            ContentTextView.AutoLinkMask = Android.Text.Util.MatchOptions.All;
            ContentTextView.LinksClickable = true;
        }

        public override void RefreshView()
        {
            if (!string.IsNullOrEmpty(ContactPreview?.Description))
            {
                Visibility = ViewStates.Visible;
                Content = ContactPreview.Description;
            }
            else
            {
                Visibility = ViewStates.Gone;
            }
        }
    }
}