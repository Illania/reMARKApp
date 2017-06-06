//
// Project: Mark5.Mobile.IOS
// File: FloatingActionButtonLayoutListener.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//

using Android.Support.Design.Widget;
using Android.Views;

namespace Mark5.Mobile.Droid.Ui.Common
{
    public class FloatingActionButtonLayoutChangeListener : Java.Lang.Object, View.IOnLayoutChangeListener
    {
        public void OnLayoutChange(View v, int left, int top, int right, int bottom, int oldLeft, int oldTop, int oldRight, int oldBottom)
        {
            var fab = v as FloatingActionButton;

            var parent = fab.Parent as CoordinatorLayout;

            if (parent == null)
                return;

            var distance = parent.Bottom - v.Bottom;
            var bottomMargin = v.Context.Resources.GetDimension(Resource.Dimension.fab_margin);

            if (distance > bottomMargin * 2)
            {
                fab.Visibility = ViewStates.Invisible; //We cannot use GONE otherwise the FAB does not get notified of scrolling anymore
                fab.Hide();
            }
            else if (!fab.IsShown)
            {
                fab.Show();
            }
        }
    }
}