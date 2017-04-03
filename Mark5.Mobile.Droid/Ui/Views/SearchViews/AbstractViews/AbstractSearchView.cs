//
// Project: ${Project}
// File: AbstractSearchView.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using Android.Content;
using Android.Graphics;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{
    public abstract class AbstractSearchView<T> : LinearLayoutCompat
    {
        public T Criteria { get; set; }

        protected int DistanceNone;
        protected int DistanceLarge;
        protected int DistanceNormal;
        protected int DistanceSmall;

        protected static Color BackgroundColorNormalState;
        protected static Color BackgroundColorSelectedState;

        protected int TextStyleTopLineResourceId = Resource.Style.searchViewTopLine;
        protected int TextStyleBottomLineResourceId = Resource.Style.searchViewBottomLine;

        protected AbstractSearchView(Context context)
                : base(context)
        {
            LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);

            DescendantFocusability = DescendantFocusability.BeforeDescendants;

            DistanceLarge = ConversionUtils.ConvertDpToPixels(16f);
            DistanceNormal = ConversionUtils.ConvertDpToPixels(8f);
            DistanceSmall = ConversionUtils.ConvertDpToPixels(4f);

            SetPadding(DistanceNormal, DistanceNormal, DistanceNormal, DistanceNormal);

            BackgroundColorNormalState = new Color(ContextCompat.GetColor(Context, Resource.Color.darkblue));
            BackgroundColorSelectedState = new Color(ContextCompat.GetColor(Context, Resource.Color.lightblue));
        }

        public abstract void Refresh();

        public abstract void UpdateCriteria();
    }
}