//
// Project: Mark5.Mobile.Droid
// File: Divider.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Android.Content;
using Android.Graphics;
using Android.Support.V4.Content;
using Android.Util;
using Android.Views;
using Android.Support.V7.Widget;

namespace Mark5.Mobile.Droid.Ui.Views.Common
{

    public class Divider : LinearLayoutCompat
    {

        public Divider(Context context)
            : base(context)
        {
            LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, (int)(TypedValue.ApplyDimension(ComplexUnitType.Dip, 0.5f, Resources.DisplayMetrics) + 0.5f));
            SetBackgroundColor(new Color(ContextCompat.GetColor(Context, Resource.Color.lightgray)));
        }

        public Divider(Context context, int leftMargin, int topMargin, int rightMargin, int bottomMargin)
            : base(context)
        {
            LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);

            var inner = new View(Context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, (int)(TypedValue.ApplyDimension(ComplexUnitType.Dip, 0.5f, Resources.DisplayMetrics) + 0.5f))
                {
                    LeftMargin = leftMargin,
                    TopMargin = topMargin,
                    RightMargin = rightMargin,
                    BottomMargin = bottomMargin
                }
            };
            inner.SetBackgroundColor(new Color(ContextCompat.GetColor(Context, Resource.Color.lightgray)));
            AddView(inner);
        }

    }
}
