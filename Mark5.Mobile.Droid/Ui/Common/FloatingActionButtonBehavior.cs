//
// Project: Mark5.Mobile.IOS
// File: FABBehavior.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using Android.Support.Design.Widget;
using Android.Views;
using Java.Interop;

namespace Mark5.Mobile.Droid.Ui.Common
{
    public class FloatingActionButtonBehavior : CoordinatorLayout.Behavior
    {
        public override bool OnStartNestedScroll(CoordinatorLayout coordinatorLayout, Java.Lang.Object child, View directTargetChild, View target, int nestedScrollAxes)
        {
            var fab = child.JavaCast<FloatingActionButton>();

            return nestedScrollAxes == (int)ScrollAxis.Vertical && (fab.Visibility != ViewStates.Visible);
        }

        public override void OnNestedScroll(CoordinatorLayout coordinatorLayout, Java.Lang.Object child, View target, int dxConsumed, int dyConsumed, int dxUnconsumed, int dyUnconsumed)
        {
            base.OnNestedScroll(coordinatorLayout, child, target, dxConsumed, dyConsumed, dxUnconsumed, dyUnconsumed);

            var fab = child.JavaCast<FloatingActionButton>();

            if (dyConsumed > 1)
            {
                fab.Show();
            }

        }
    }
}
