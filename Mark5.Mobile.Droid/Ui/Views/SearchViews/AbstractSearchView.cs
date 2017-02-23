//
// Project: Mark5.Mobile.Droid
// File: AbstractSearchView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{

    public abstract class AbstractSearchView<T> : LinearLayoutCompat
    {

        protected int DistanceNone;
        protected int DistanceLarge;
        protected int DistanceNormal;
        protected int DistanceSmall;

        protected AbstractSearchView(Context context)
            : base(context)
        {
            LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);

            DistanceLarge = ConversionUtils.ConvertDpToPixels(16f);
            DistanceNormal = ConversionUtils.ConvertDpToPixels(8f);
            DistanceSmall = ConversionUtils.ConvertDpToPixels(4f);
        }

        public abstract void FromCriteria(T criteria);

        public abstract void ToCriteria(T criteria);
    }
}
