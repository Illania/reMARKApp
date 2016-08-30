//
// Project: Mark5.Mobile.Droid
// File: Ui.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using Android.OS;
using Android.Support.V7.App;

namespace Mark5.Mobile.Droid.Utilities
{
    public static class Ui
    {

        public static void RunOnUiThread(AppCompatActivity activity, Action a)
        {
            if (Looper.MainLooper == Looper.MyLooper())
            {
                a();
            }
            else
            {
                activity.RunOnUiThread(a);
            }
        }
    }
}

