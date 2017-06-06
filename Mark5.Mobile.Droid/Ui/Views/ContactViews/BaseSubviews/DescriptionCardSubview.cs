using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views.ContactViews
{
    public abstract class DescriptionCardSubview : ContactView
    {
        readonly AppCompatTextView titleTextView;
        protected readonly AppCompatTextView ContentTextView;

        public string Title { set => titleTextView.Text = value; }

        public string Content { set => ContentTextView.Text = value; }

        protected DescriptionCardSubview(Context context)
            : base(context)
        {
            Orientation = Vertical;

            LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            SetPadding(DistanceVeryLarge, DistanceNormal, DistanceNormal, DistanceNormal);

            LongClickable = true;
            LongClick += (sender, e) =>
            {
                if (!string.IsNullOrWhiteSpace(ContentTextView.Text))
                    Integration.CopyToClipboard(context, ContentTextView.Text);
            };

            titleTextView = new AppCompatTextView(context);
            titleTextView.SetTextAppearanceCompat(context, Resource.Style.fontPrimary);
            AddView(titleTextView, new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));

            ContentTextView = new AppCompatTextView(context);
            ContentTextView.SetTextAppearanceCompat(context, Resource.Style.fontSmallLight);
            AddView(ContentTextView, new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));
        }
    }
}