//
// Project: Mark5.Mobile.Droid
// File: ObjectLinksView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Activities;
using Mark5.Mobile.Droid.Ui.Common;
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

        public event EventHandler<ObjectLink> ObjectLinkClicked = delegate { };

        public ObjectLinksView(Context context, string title, ObjectLink[] objectLinks)
            : base(context)
        {
            InitializeView(title, objectLinks);
        }

        void InitializeView(string title, ObjectLink[] objectLinks)
        {
            LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            Elevation = (int)(TypedValue.ApplyDimension(ComplexUnitType.Dip, 2f, Resources.DisplayMetrics) + 0.5f);
            Radius = (int)(TypedValue.ApplyDimension(ComplexUnitType.Dip, 2f, Resources.DisplayMetrics) + 0.5f);
            UseCompatPadding = true;

            distanceVeryLarge = (int)(TypedValue.ApplyDimension(ComplexUnitType.Dip, 24f, Resources.DisplayMetrics) + 0.5f);
            distanceLarge = (int)(TypedValue.ApplyDimension(ComplexUnitType.Dip, 16f, Resources.DisplayMetrics) + 0.5f);
            distanceNormal = (int)(TypedValue.ApplyDimension(ComplexUnitType.Dip, 8f, Resources.DisplayMetrics) + 0.5f);
            distanceSmall = (int)(TypedValue.ApplyDimension(ComplexUnitType.Dip, 4f, Resources.DisplayMetrics) + 0.5f);

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

            foreach (var objectLink in objectLinks)
            {
                var olv = new ObjectLinkView(Context, objectLink, distanceVeryLarge, distanceNormal);
                olv.Click += (sender, e) => ObjectLinkClicked(this, objectLink);
                innerLayout.AddView(olv);
            }
        }

        class ObjectLinkView : LinearLayoutCompat
        {

            public ObjectLinkView(Context context, ObjectLink ol, int distanceVeryLarge, int distanceNormal)
                : base(context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
                Orientation = Vertical;
                SetPadding(distanceVeryLarge, distanceNormal, distanceVeryLarge, distanceNormal);

                var titleView = new AppCompatTextView(Context)
                {
                    LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                    Text = ol.IsReverse ? ol.TypeInfo.DescriptionComplexReverse : ol.TypeInfo.DescriptionComplex
                };
                titleView.SetTextAppearanceCompat(Context, Resource.Style.fontPrimary);
                AddView(titleView);

                var subtitleView = new AppCompatTextView(Context)
                {
                    LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                    Text = ol.Description
                };
                subtitleView.SetTextAppearanceCompat(Context, Resource.Style.fontSmallLight);
                AddView(subtitleView);

                if (ol.IsReverse)
                {   
                    Clickable = (ol.FromObjectType == ObjectType.Document || ol.FromObjectType == ObjectType.Contact || ol.FromObjectType == ObjectType.Shortcode);
                    if (Clickable)
                    {
                        var typedArray = Context.ObtainStyledAttributes(new int[] { Resource.Attribute.selectableItemBackground });
                        SetBackgroundResource(typedArray.GetResourceId(0, 0));
                        typedArray.Recycle();
                    }
                }
                else
                {
                    Clickable = (ol.ToObjectType == ObjectType.Document || ol.ToObjectType == ObjectType.Contact || ol.ToObjectType == ObjectType.Shortcode);
                    if (Clickable)
                    {
                        var typedArray = Context.ObtainStyledAttributes(new int[] { Resource.Attribute.selectableItemBackground });
                        SetBackgroundResource(typedArray.GetResourceId(0, 0));
                        typedArray.Recycle();
                    }
                }
            }
        }

    }
}
