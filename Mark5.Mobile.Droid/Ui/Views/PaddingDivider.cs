//
// Project: 
// File: PaddingDivider.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views
{
    public class PaddingDivider : LinearLayoutCompat
    {
        public PaddingDivider(Context context, int paddingLeft, int paddingRight)
            : base(context)
        {
            LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            SetPadding(ConversionUtils.ConvertDpToPixels(paddingLeft), 0, ConversionUtils.ConvertDpToPixels(paddingRight), 0);
            AddView(new Divider(context));
        }
    }
}
