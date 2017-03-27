//
// Project: Mark5.Mobile.Droid
// File: OnFocusChangeListener.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using Android.Views;

namespace Mark5.Mobile.Droid.Ui.Common
{
    public class OnFocusChangeListener : Java.Lang.Object, View.IOnFocusChangeListener
    {
        readonly Action<View, bool> action;

        public OnFocusChangeListener(Action<View, bool> action)
        {
            this.action = action;
        }

        public void OnFocusChange(View v, bool hasFocus)
        {
            if (action != null)
            {
                action(v, hasFocus);
            }
        }
    }
}
