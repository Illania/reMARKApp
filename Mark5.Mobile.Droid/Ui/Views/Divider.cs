//
// Project: Mark5.Mobile.Droid
// File: Divider.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Android.Content;
using Android.Support.V4.Content;
using Android.Util;
using Android.Views;

namespace Mark5.Mobile.Droid.Ui.Views
{

    public class Divider : View
    {

        public Divider(Context context)
            : base(context)
        {
            LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, (int)(TypedValue.ApplyDimension(ComplexUnitType.Dip, 0.5f, Resources.DisplayMetrics) + 0.5f));
            Background = ContextCompat.GetDrawable(Context, Resource.Drawable.line_divider);
        }
    }
}
