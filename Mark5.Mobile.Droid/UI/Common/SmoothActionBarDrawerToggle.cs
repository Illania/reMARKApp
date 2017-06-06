using System;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Support.V4.Widget;

namespace Mark5.Mobile.Droid.Ui.Common
{
    public class SmoothActionBarDrawerToggle : ActionBarDrawerToggle
    {
        public Action action;

        public SmoothActionBarDrawerToggle(IntPtr javaReference, Android.Runtime.JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        public SmoothActionBarDrawerToggle(Android.App.Activity activity, Android.Support.V4.Widget.DrawerLayout drawerLayout, Toolbar toolbar, int openDrawerContentDescRes, int closeDrawerContentDescRes)
            : base(activity, drawerLayout, toolbar, openDrawerContentDescRes, closeDrawerContentDescRes)
        {
        }

        public SmoothActionBarDrawerToggle(Android.App.Activity activity, Android.Support.V4.Widget.DrawerLayout drawerLayout, int openDrawerContentDescRes, int closeDrawerContentDescRes)
            : base(activity, drawerLayout, openDrawerContentDescRes, closeDrawerContentDescRes)
        {
        }

        public override void OnDrawerStateChanged(int newState)
        {
            base.OnDrawerStateChanged(newState);

            if (action != null && newState == DrawerLayout.StateIdle)
            {
                action();
                action = null;
            }
        }

        public void RunWhenIdle(Action action, bool forceRun = false)
        {
            if (forceRun)
                action();
            else
                this.action = action;
        }
    }
}