//
// Project: Mark5.Mobile.Droid
// File: ObjectActionsView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Android.Content;
using Android.Views;
using System.Collections.Generic;
using Mark5.Mobile.Common.Model;
using Android.Support.V7.Widget;
using Android.Util;
using Android.OS;
using Android.Graphics;
using Android.Support.V4.Content;
using Mark5.Mobile.Droid.Ui.Views.Common;
using System;
using System.Linq;
using Mark5.Mobile.Droid.Utilities;
using Android.Text.Format;

namespace Mark5.Mobile.Droid.Ui.Views
{

    public class ObjectActionsView : CardView
    {
        int distanceNone;
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
            Elevation = (int)(TypedValue.ApplyDimension(ComplexUnitType.Dip, 2.0f, Resources.DisplayMetrics) + 0.5f);
            Radius = (int)(TypedValue.ApplyDimension(ComplexUnitType.Dip, 2.0f, Resources.DisplayMetrics) + 0.5f);
            UseCompatPadding = true;

            distanceVeryLarge = (int)(TypedValue.ApplyDimension(ComplexUnitType.Dip, 24.0f, Resources.DisplayMetrics) + 0.5f);
            distanceLarge = (int)(TypedValue.ApplyDimension(ComplexUnitType.Dip, 16.0f, Resources.DisplayMetrics) + 0.5f);
            distanceNormal = (int)(TypedValue.ApplyDimension(ComplexUnitType.Dip, 8.0f, Resources.DisplayMetrics) + 0.5f);
            distanceSmall = (int)(TypedValue.ApplyDimension(ComplexUnitType.Dip, 4.0f, Resources.DisplayMetrics) + 0.5f);

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
            if (Build.VERSION.SdkInt < BuildVersionCodes.M)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                titleView.SetTextAppearance(Context, Resource.Style.fontLarge);
#pragma warning restore CS0618 // Type or member is obsolete
            }
            else
            {
#pragma warning disable XA0001 // Find issues with Android API usage
                titleView.SetTextAppearance(Resource.Style.fontLarge);
#pragma warning restore XA0001 // Find issues with Android API usage
            }
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
                if (Build.VERSION.SdkInt < BuildVersionCodes.M)
                {
#pragma warning disable CS0618 // Type or member is obsolete
                    titleView.SetTextAppearance(Context, Resource.Style.fontPrimary);
#pragma warning restore CS0618 // Type or member is obsolete
                }
                else
                {
#pragma warning disable XA0001 // Find issues with Android API usage
                    titleView.SetTextAppearance(Resource.Style.fontPrimary);
#pragma warning restore XA0001 // Find issues with Android API usage
                }
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
                if (Build.VERSION.SdkInt < BuildVersionCodes.M)
                {
#pragma warning disable CS0618 // Type or member is obsolete
                    subtitleView.SetTextAppearance(Context, Resource.Style.fontSmallLight);
#pragma warning restore CS0618 // Type or member is obsolete
                }
                else
                {
#pragma warning disable XA0001 // Find issues with Android API usage
                    subtitleView.SetTextAppearance(Resource.Style.fontSmallLight);
#pragma warning restore XA0001 // Find issues with Android API usage
                }
                AddView(subtitleView);
            }
        }

    }
}
