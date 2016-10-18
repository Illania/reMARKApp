//
// Project: Mark5.Mobile.Droid
// File: ObjectLinksView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Views.Common;

namespace Mark5.Mobile.Droid.Ui.Views
{

    public class ObjectLinksView : CardView
    {

        int distanceVeryLarge;
        int distanceLarge;
        int distanceNormal;
        int distanceSmall;

        LinearLayoutCompat innerLayout;

        public ObjectLinksView(Context context, string title, ObjectLink[] objectLinks)
            : base(context)
        {
            InitializeView(title, objectLinks);
        }

        void InitializeView(string title, ObjectLink[] objectLinks)
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

            for (int i = 0; i < objectLinks.Length; i++)
            {
                var objectLink = objectLinks[i];
                var isNotLast = i != objectLinks.Length - 1;

                innerLayout.AddView(new ObjectLinkView(Context, objectLink, distanceVeryLarge, distanceNormal));

                if (isNotLast)
                {
                    innerLayout.AddView(new Divider(Context, distanceVeryLarge, 0, 0, 0));
                }
            }
        }

        class ObjectLinkView : LinearLayoutCompat
        {

            public ObjectLinkView(Context context, ObjectLink objectLink, int distanceVeryLarge, int distanceNormal)
                : base(context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
                Orientation = Vertical;
                SetPadding(distanceVeryLarge, distanceNormal, distanceVeryLarge, distanceNormal);

                var titleView = new AppCompatTextView(Context)
                {
                    LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                    Text = objectLink.Description
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

                var subtitleView = new AppCompatTextView(Context)
                {
                    LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                    Text = objectLink.TypeInfo.DescriptionComplex
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
