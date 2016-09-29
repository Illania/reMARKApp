//
// Project: Mark5.Mobile.Droid
// File: Divider.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Android.Content;
using Android.Support.V4.Content;
using Android.Views;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views
{

    public class Divider : View
    {

        public Divider(Context context)
            : base(context)
        {
            LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ConversionUtils.ConvertDpToPixels(1f));
            Background = ContextCompat.GetDrawable(Context, Resource.Drawable.line_divider);
        }
    }
}
