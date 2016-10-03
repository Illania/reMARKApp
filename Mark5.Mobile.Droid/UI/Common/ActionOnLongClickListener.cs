//
// Project: Mark5.Mobile.Droid
// File: ActionOnLongClickListener.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using Android.Views;

namespace Mark5.Mobile.Droid.Ui.Common
{

    public class ActionOnLongClickListener : Java.Lang.Object, View.IOnLongClickListener
    {

        readonly Action action;

        public ActionOnLongClickListener(Action action)
        {
            this.action = action;
        }

        public bool OnLongClick(View v)
        {
            if (action != null)
            {
                action();
                return true;
            }

            return false;
        }
    }
}
