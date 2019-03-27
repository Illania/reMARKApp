using Android.Content;
using Android.Graphics;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views
{
    public class ObjectActionsView : CardView
    {
        int distanceVeryLarge;
        int distanceLarge;
        int distanceNormal;
        int distanceSmall;

        LinearLayoutCompat innerLayout;

        public ObjectActionsView(Context context, string title, ObjectAction[] objectActions)
            : base(context)
        {
            InitializeView(title, objectActions);
        }

        void InitializeView(string title, ObjectAction[] objectActions)
        {
            LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            Elevation = 0;
            Radius = Conversion.ConvertDpToPixels(2f);
            UseCompatPadding = true;

            distanceVeryLarge = Conversion.ConvertDpToPixels(24f);
            distanceLarge = Conversion.ConvertDpToPixels(16f);
            distanceNormal = Conversion.ConvertDpToPixels(8f);
            distanceSmall = Conversion.ConvertDpToPixels(4f);

            innerLayout = new LinearLayoutCompat(Context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                Orientation = LinearLayoutCompat.Vertical
            };
            innerLayout.SetPadding(0, distanceLarge, 0, distanceLarge);
            AddView(innerLayout);

            var titleView = new AppCompatTextView(Context)
            {
                LayoutParameters = new LinearLayoutCompat.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
                {
                    BottomMargin = distanceSmall
                },
                Text = title
            };
            titleView.SetTextAppearanceCompat(Context, Resource.Style.fontLarge);

            titleView.SetTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.darkblue)));
            titleView.SetPadding(distanceVeryLarge, 0, distanceNormal, 0);
            innerLayout.AddView(titleView);

            foreach (var objectAction in objectActions)
                innerLayout.AddView(new ObjectActionView(Context, objectAction, distanceVeryLarge, distanceNormal));
        }

        class ObjectActionView : LinearLayoutCompat
        {
            public ObjectActionView(Context context, ObjectAction objectAction, int distanceVeryLarge, int distanceNormal)
                : base(context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
                Orientation = Vertical;
                SetPadding(distanceVeryLarge, distanceNormal, distanceVeryLarge, distanceNormal);

                var processedActionTimeTimestamp = objectAction.ActionTimeTimestamp.ConvertTimestampMillisecondsToDateTime().ConvertUtcToUserTime().ConvertDateTimeToTimestampMilliseconds();

                var titleView = new AppCompatTextView(Context)
                {
                    LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                    Text = $"{objectAction.Description} {Context.GetString(Resource.String.on)} {processedActionTimeTimestamp.FormatUserTimestampAsTimeAndDateString(context)}"
                };
                titleView.SetTextAppearanceCompat(Context, Resource.Style.fontPrimary);

                AddView(titleView);

                var subtitleView = new AppCompatTextView(Context)
                {
                    LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                    Text = objectAction.ActionType
                };
                subtitleView.SetTextAppearanceCompat(Context, Resource.Style.fontSmallLight);

                AddView(subtitleView);
            }
        }
    }
}