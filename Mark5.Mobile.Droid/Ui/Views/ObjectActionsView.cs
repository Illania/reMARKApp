//
// Project: Mark5.Mobile.Droid
// File: ObjectActionsView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Android.Content;
using Android.Graphics;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
using Android.Text.Format;
using Android.Views;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Views.Common;
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
            Elevation = ConversionUtils.ConvertDpToPixels(2.0f);
            Radius = ConversionUtils.ConvertDpToPixels(2.0f);
            UseCompatPadding = true;

            distanceVeryLarge = ConversionUtils.ConvertDpToPixels(24.0f);
            distanceLarge = ConversionUtils.ConvertDpToPixels(16.0f);
            distanceNormal = ConversionUtils.ConvertDpToPixels(8.0f);
            distanceSmall = ConversionUtils.ConvertDpToPixels(4.0f);

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

            titleView.SetTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.darkerblue)));
            titleView.SetPadding(distanceLarge, 0, distanceNormal, 0);
            innerLayout.AddView(titleView);

            innerLayout.AddView(new Divider(Context));

            for (int i = 0; i < objectActions.Length; i++)
            {
                var objectAction = objectActions[i];
                var isNotLast = i != objectActions.Length - 1;

                innerLayout.AddView(new ObjectActionView(Context, objectAction, distanceVeryLarge, distanceNormal));

                if (isNotLast)
                {
                    innerLayout.AddView(new Divider(Context, distanceVeryLarge, 0, 0, 0));
                }
            }
        }

        class ObjectActionView : LinearLayoutCompat
        {

            public ObjectActionView(Context context, ObjectAction objectAction, int distanceVeryLarge, int distanceNormal)
                : base(context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
                Orientation = Vertical;
                SetPadding(distanceVeryLarge, distanceNormal, distanceVeryLarge, distanceNormal);

                var titleView = new AppCompatTextView(Context)
                {
                    LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                    Text = objectAction.Description
                };
                titleView.SetTextAppearanceCompat(Context, Resource.Style.fontPrimary);

                AddView(titleView);

                var actionTime = objectAction.ActionTime.ToServerTime();

                var dfo = DateFormat.GetDateFormatOrder(context);
                var actionDateString = actionTime.ToString($"{dfo[0]}{dfo[0]}/{dfo[1]}{dfo[1]}/{dfo[2]}{dfo[2]}{dfo[2]}{dfo[2]}");
                var actionTimeString = DateFormat.Is24HourFormat(context) ? actionTime.ToString("HH:mm") : actionTime.ToString("hh:mm tt");

                var subtitleView = new AppCompatTextView(Context)
                {
                    LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                    Text = $"{Context.GetString(Resource.String.by)} {objectAction.Username} {Context.GetString(Resource.String.on)} {actionDateString} {actionTimeString}"
                };
                subtitleView.SetTextAppearanceCompat(Context, Resource.Style.fontSmallLight);

                AddView(subtitleView);
            }
        }

    }
}
