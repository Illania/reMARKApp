//
// Project: Mark5.Mobile.Droid
// File: BaseAppCompatActivity.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;

namespace Mark5.Mobile.Droid.Ui.Common
{

    public abstract class BaseAppCompatActivity : AppCompatActivity
    {

        protected override void AttachBaseContext(Context @base)
        {
            //base.AttachBaseContext(UK.CO.Chrisjenx.Calligraphy.CalligraphyContextWrapper.Wrap(@base));
            base.AttachBaseContext(@base);
        }

        protected void RunOnUiThreadIfNecessary(Action a)
        {
            if (Looper.MainLooper == Looper.MyLooper())
            {
                a();
            }
            else
            {
                RunOnUiThread(a);
            }
        }
    }
}

