using System;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Widget;
using AndroidX.DrawerLayout.Widget;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace reMark.Mobile.Droid.Ui.Common
{
    public class SmoothActionBarDrawerToggle : ActionBarDrawerToggle
    {
        public Action action;

        public SmoothActionBarDrawerToggle(IntPtr javaReference, Android.Runtime.JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        public SmoothActionBarDrawerToggle(Android.App.Activity activity, DrawerLayout drawerLayout, Toolbar toolbar, int openDrawerContentDescRes, int closeDrawerContentDescRes)
            : base(activity, drawerLayout, toolbar, openDrawerContentDescRes, closeDrawerContentDescRes)
        {
        }

        public SmoothActionBarDrawerToggle(Android.App.Activity activity, DrawerLayout drawerLayout, int openDrawerContentDescRes, int closeDrawerContentDescRes)
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