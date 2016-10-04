//
// Project: Mark5.Mobile.Droid
// File: ShortcodeView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Android.Content;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Ui.Views.ShortcodeViews
{

    public abstract class ShortcodeView : LinearLayoutCompat
    {

        public ShortcodePreview ShortcodePreview { get; set; }

        public Shortcode Shortcode { get; set; }

        protected int DistanceNone;
        protected int DistanceLarge;
        protected int DistanceNormal;
        protected int DistanceSmall;

        public ShortcodeView(Context context, IAttributeSet attrs, int defStyleAttr)
            : base(context, attrs, defStyleAttr)
        {
            Init();
        }

        public ShortcodeView(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
            Init();
        }

        protected ShortcodeView(Context context)
            : base(context)
        {
            Init();
        }

        void Init()
        {
            LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);

            DistanceLarge = (int)(TypedValue.ApplyDimension(ComplexUnitType.Dip, 16.0f, Resources.DisplayMetrics) + 0.5f);
            DistanceNormal = (int)(TypedValue.ApplyDimension(ComplexUnitType.Dip, 8.0f, Resources.DisplayMetrics) + 0.5f);
            DistanceSmall = (int)(TypedValue.ApplyDimension(ComplexUnitType.Dip, 4.0f, Resources.DisplayMetrics) + 0.5f);

            Visibility = ViewStates.Gone;
        }

        public abstract void RefreshView();
    }
}

