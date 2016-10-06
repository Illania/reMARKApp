//
// Project: 
// File: HeaderViewBehavior.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//
using System;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views.ContactViews
{
    [Register("contact.HeaderView.behavior")]
    public class HeaderViewBehavior : CoordinatorLayout.Behavior
    {
        readonly Context mContext;

        int mStartMarginLeft;
        int mEndMargintLeft;
        int mMarginRight;
        int mStartMarginBottom;
        bool isHide;

        public HeaderViewBehavior(Context context, IAttributeSet attrs)
        {
            mContext = context;

            mStartMarginLeft = ConversionUtils.ConvertDpToPixels(16);
            mEndMargintLeft = ConversionUtils.ConvertDpToPixels(40);
            mMarginRight = ConversionUtils.ConvertDpToPixels(14);
            mStartMarginBottom = ConversionUtils.ConvertDpToPixels(14);
        }

        public override bool LayoutDependsOn(CoordinatorLayout parent, Java.Lang.Object child, Android.Views.View dependency)
        {
            return dependency is AppBarLayout;
        }

        public override bool OnDependentViewChanged(CoordinatorLayout parent, Java.Lang.Object child, Android.Views.View dependency)
        {
            var headerView = child.JavaCast<ContactHeaderView>();

            //headerView.SetTitles("bla", "Medina");

            int maxScroll = ((AppBarLayout)dependency).TotalScrollRange;
            float percentage = Math.Abs(dependency.GetY()) / maxScroll;

            float childPosition = dependency.Height
                                            + dependency.GetY()
                                            - headerView.Height
                                            - (GetToolbarHeight() - headerView.Height) * percentage / 2;


            childPosition = childPosition - mStartMarginBottom * (1f - percentage);

            var lp = (CoordinatorLayout.LayoutParams)headerView.LayoutParameters;
            lp.LeftMargin = (int)(percentage * mEndMargintLeft) + mStartMarginLeft;
            lp.RightMargin = mMarginRight;
            headerView.LayoutParameters = lp;

            headerView.SetY(childPosition);

            if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
            {
                if (isHide && percentage < 1)
                {
                    headerView.Visibility = ViewStates.Visible;
                    isHide = false;
                }
                else if (!isHide && percentage >= 1)
                {
                    headerView.Visibility = ViewStates.Gone;
                    isHide = true;
                }
            }
            return true;

        }

        public int GetToolbarHeight()
        {
            int result = 0;
            var tv = new TypedValue();
            if (mContext.Theme.ResolveAttribute(Resource.Attribute.actionBarSize, tv, true))
            {
                result = TypedValue.ComplexToDimensionPixelSize(tv.Data, mContext.Resources.DisplayMetrics);
            }
            return result;
        }
    }
}
