using System;
using Android.Content;
using Android.Util;
using Android.Views;
using AndroidX.ViewPager.Widget;

namespace reMark.Mobile.Droid.Ui.Common
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
            catch(Java.Lang.IllegalArgumentException)
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
            catch (Java.Lang.IllegalArgumentException)
            {
                //ignore
            }
            return false;
        }
    }
}
