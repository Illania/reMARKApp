using System;
using Android.Content;
using Android.Support.V4.View;
using Android.Util;
using Android.Views;

namespace Mark5.Mobile.Droid.Ui.Common
{
    public class ViewPagerFixed : ViewPager
    {
        public ViewPagerFixed(Context context) : base(context)
        {
        }

        public ViewPagerFixed(Context context, IAttributeSet attrs) : base(context, attrs)
        {
        }


        public override bool OnTouchEvent(MotionEvent ev)
        {
            try
            {
                return base.OnTouchEvent(ev);
            }
            catch (ArgumentOutOfRangeException)
            {
                //ignore
            }
            return false;
        }


        public override bool OnInterceptTouchEvent(MotionEvent ev)
        {
            try
            {
                return base.OnInterceptTouchEvent(ev);
            }
            catch (ArgumentOutOfRangeException)
            {
                //ignore
            }
            return false;
        }
    }
}
